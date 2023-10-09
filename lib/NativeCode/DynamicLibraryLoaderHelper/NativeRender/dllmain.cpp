// dllmain.cpp : Defines the entry point for the DLL application.
// This file does some *magick* to load the EOS Overlay DLL.
// This is apparently needed so that the Overlay can render properly
#include "pch.h"

#define _SILENCE_CXX17_CODECVT_HEADER_DEPRECATION_WARNING

#include <stdio.h>
#include <assert.h>
#include <stdlib.h>

#include <string>
#include <sstream>
#include <functional>
#include <algorithm>
#include <utility>
#include <filesystem>
#include <optional>
#include <codecvt>


//#include "eos_minimum_includes.h"
#if PLATFORM_WINDOWS
#include "Windows/eos_Windows_base.h"
#include "Windows/eos_Windows.h"
#include "processenv.h"
#include <iterator>
#endif
#include "eos_sdk.h"
#include "eos_logging.h"
#include "eos_integratedplatform.h"

#include "json.h"

// This define exists because UWP
// Originally, this would load the library with the name as shipped by the .zip file
#define USE_PLATFORM_WITHOUT_BITS 0

#if USE_PLATFORM_WITHOUT_BITS
#define DLL_PLATFORM "-Win"
#else
#if PLATFORM_64BITS
#define DLL_PLATFORM "-Win64"
#else
#define DLL_PLATFORM "-Win32"
#endif
#endif

#if PLATFORM_64BITS
#define STEAM_DLL_NAME "steam_api64.dll"
#else
#define STEAM_DLL_NAME "steam_api.dll"
#endif

#define DLL_SUFFIX "-Shipping.dll"

#define SHOW_DIALOG_BOX_ON_WARN 0
#define ENABLE_DLL_BASED_EOS_CONFIG 1
#define OVERLAY_DLL_NAME "EOSOVH" DLL_PLATFORM DLL_SUFFIX
#define SDK_DLL_NAME "EOSSDK" DLL_PLATFORM DLL_SUFFIX
#define XAUDIO2_DLL_NAME "xaudio2_9redist.dll"

#define EOS_SERVICE_CONFIG_FILENAME "EpicOnlineServicesConfig.json"
#define EOS_STEAM_CONFIG_FILENAME "eos_steam_config.json"

#define RESTRICT __restrict

#define DLL_EXPORT(return_value) extern "C" __declspec(dllexport) return_value  __stdcall

namespace fs = std::filesystem;
typedef HKEY__* HKEY;

using FSig_ApplicationWillShutdown = void (__stdcall *)(void);
FSig_ApplicationWillShutdown FuncApplicationWillShutdown = nullptr;

typedef const char* (*GetConfigAsJSONString_t)();

//-------------------------------------------------------------------------
// Fetched out of DLLs
typedef EOS_EResult(EOS_CALL* EOS_Initialize_t)(const EOS_InitializeOptions* Options);
typedef EOS_EResult(EOS_CALL* EOS_Shutdown_t)();
typedef EOS_HPlatform(EOS_CALL* EOS_Platform_Create_t)(const EOS_Platform_Options* Options);
typedef void (EOS_CALL* EOS_Platform_Release_t)(EOS_HPlatform Handle);
typedef EOS_EResult (EOS_CALL *EOS_Logging_SetLogLevel_t)(EOS_ELogCategory LogCategory, EOS_ELogLevel LogLevel);
typedef EOS_EResult (EOS_CALL *EOS_Logging_SetCallback_t)(EOS_LogMessageFunc Callback);

typedef EOS_EResult (*EOS_IntegratedPlatformOptionsContainer_Add_t)(EOS_HIntegratedPlatformOptionsContainer Handle, const EOS_IntegratedPlatformOptionsContainer_AddOptions* InOptions);
typedef EOS_EResult (*EOS_IntegratedPlatform_CreateIntegratedPlatformOptionsContainer_t)(const EOS_IntegratedPlatform_CreateIntegratedPlatformOptionsContainerOptions* Options, EOS_HIntegratedPlatformOptionsContainer* OutIntegratedPlatformOptionsContainerHandle);
typedef void (*EOS_IntegratedPlatformOptionsContainer_Release_t)(EOS_HIntegratedPlatformOptionsContainer IntegratedPlatformOptionsContainerHandle);

static EOS_Initialize_t EOS_Initialize_ptr;
static EOS_Shutdown_t EOS_Shutdown_ptr;
static EOS_Platform_Create_t EOS_Platform_Create_ptr;
static EOS_Platform_Release_t EOS_Platform_Release_ptr;
static EOS_Logging_SetLogLevel_t EOS_Logging_SetLogLevel_ptr;
static EOS_Logging_SetCallback_t EOS_Logging_SetCallback_ptr;

static EOS_IntegratedPlatformOptionsContainer_Add_t EOS_IntegratedPlatformOptionsContainer_Add_ptr;
static EOS_IntegratedPlatform_CreateIntegratedPlatformOptionsContainer_t EOS_IntegratedPlatform_CreateIntegratedPlatformOptionsContainer_ptr;
static EOS_IntegratedPlatformOptionsContainer_Release_t EOS_IntegratedPlatformOptionsContainer_Release_ptr;

static void *s_eos_sdk_overlay_lib_handle;
static void *s_eos_sdk_lib_handle;
static EOS_HPlatform eos_platform_handle;
static GetConfigAsJSONString_t GetConfigAsJSONString;

struct SandboxDeploymentOverride
{
    std::string sandboxID;
    std::string deploymentID;
};

struct EOSConfig
{
    std::string productName;
    std::string productVersion;

    std::string productID;
    std::string sandboxID;
    std::string deploymentID;
    std::vector<SandboxDeploymentOverride> sandboxDeploymentOverrides;

    std::string clientSecret;
    std::string clientID;
    std::string encryptionKey;

    std::string overrideCountryCode;
    std::string overrideLocaleCode;

    // this is called platformOptionsFlags in C#
    uint64_t flags = 0;

    uint32_t tickBudgetInMilliseconds = 0;

    uint64_t ThreadAffinity_networkWork = 0;
    uint64_t ThreadAffinity_storageIO = 0;
    uint64_t ThreadAffinity_webSocketIO = 0;
    uint64_t ThreadAffinity_P2PIO = 0;
    uint64_t ThreadAffinity_HTTPRequestIO = 0;
    uint64_t ThreadAffinity_RTCIO = 0;

    bool isServer = false;

};

struct EOSSteamConfig
{
    EOS_EIntegratedPlatformManagementFlags flags;
    uint32_t steamSDKMajorVersion;
    uint32_t steamSDKMinorVersion;
    std::optional<std::string> OverrideLibraryPath;

    EOSSteamConfig()
    {
        flags = static_cast<EOS_EIntegratedPlatformManagementFlags>(0);
    }

    bool isManagedByApplication()
    {
        return std::underlying_type<EOS_EIntegratedPlatformManagementFlags>::type(flags & EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_LibraryManagedByApplication);
    }
    bool isManagedBySDK()
    {
        return std::underlying_type<EOS_EIntegratedPlatformManagementFlags>::type(flags & EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_LibraryManagedBySDK);
    }

};

extern "C"
{
    void __declspec(dllexport) __stdcall UnityPluginLoad(void* unityInterfaces);
    void __declspec(dllexport) __stdcall UnityPluginUnload();

    void __declspec(dllexport) __stdcall UnloadEOS();
}

//-------------------------------------------------------------------------
static bool create_timestamp_str(char *final_timestamp, size_t final_timestamp_len)
{
    constexpr size_t buffer_len = 32;
    char buffer[buffer_len];

    if (buffer_len > final_timestamp_len)
    {
        return false;
    }

    time_t raw_time = time(NULL);
    tm time_info = { 0 };

    timespec time_spec = { 0 };
    timespec_get(&time_spec, TIME_UTC);
    localtime_s(&time_info, &raw_time);

    strftime(buffer, buffer_len, "%Y-%m-%dT%H:%M:%S", &time_info);
    long milliseconds = (long)round(time_spec.tv_nsec / 1.0e6);
    snprintf(final_timestamp, final_timestamp_len, "%s.%03ld", buffer, milliseconds);

    return true;
}

//-------------------------------------------------------------------------
size_t utf8_str_bytes_required_for_wide_str(const wchar_t* wide_str, int wide_str_len = -1)
{
    int bytes_required = WideCharToMultiByte(CP_UTF8, 0, wide_str, wide_str_len, NULL, 0, NULL, NULL);

    if (bytes_required < 0)
    {
        return 0;
    }

    return bytes_required;
}

//-------------------------------------------------------------------------
// wide_str must be null terminated if wide_str_len is passed
static bool copy_to_utf8_str_from_wide_str(char* RESTRICT utf8_str, size_t utf8_str_len, const wchar_t* RESTRICT wide_str, int wide_str_len = -1)
{
    if (utf8_str_len > INT_MAX)
    {
        return false;
    }

    WideCharToMultiByte(CP_UTF8, 0, wide_str, wide_str_len, utf8_str, (int)utf8_str_len, NULL, NULL);

    return true;
}

//-------------------------------------------------------------------------
static char* create_utf8_str_from_wide_str(const wchar_t *wide_str)
{
    const int wide_str_len = (int)wcslen(wide_str) + 1;
    int bytes_required = (int)utf8_str_bytes_required_for_wide_str(wide_str, wide_str_len);
    char *to_return = (char*)malloc(bytes_required);

    if (!copy_to_utf8_str_from_wide_str(to_return, bytes_required, wide_str, wide_str_len))
    {
        free(to_return);
        to_return = NULL;
    }

    return to_return;
}

//-------------------------------------------------------------------------
static wchar_t* create_wide_str_from_utf8_str(const char* utf8_str)
{
    int chars_required = MultiByteToWideChar(CP_UTF8, 0, utf8_str, -1, NULL, 0);
    wchar_t *to_return = (wchar_t*)malloc(chars_required * sizeof(wchar_t));
    int utf8_str_len = (int)strlen(utf8_str);

    MultiByteToWideChar(CP_UTF8, 0, utf8_str, utf8_str_len, to_return, chars_required);

    return to_return;
}

//-------------------------------------------------------------------------
// Using the std::wstring_convert method for this currently. It might be the
// case that in the future this method won't work. If that happens,
// one could convert this function to use the create_utf8_str_from_wide_str
// function to emulate it. Doing this might come with a cost, as data will
// need to be copied multiple times.
static std::string to_utf8_str(const std::wstring& wide_str)
{
    std::wstring_convert<std::codecvt_utf8<wchar_t>> converter;
    std::string utf8_str = converter.to_bytes(wide_str);

    return utf8_str;

}

//-------------------------------------------------------------------------
// Using fs::path:string().c_str() seems to cause an issue when paths have
// kanji in them. Using this function and then std::string:c_str() works around that
// issue
static std::string to_utf8_str(const fs::path& path)
{
    return to_utf8_str(path.native());
}

//-------------------------------------------------------------------------
static uint64_t json_value_as_uint64(json_value_s *value, uint64_t default_value = 0)
{
    uint64_t val = 0;
    json_number_s *n = json_value_as_number(value);

    if (n != nullptr)
    {
        char *end = nullptr;
        val = strtoull(n->number, &end, 10);
    }
    else
    {
        // try to treat it as a string, then parse as long
        char *end = nullptr;
        json_string_s* val_as_str = json_value_as_string(value);
        if (val_as_str == nullptr || strlen(val_as_str->string) == 0)
        {
            val = default_value;
        }
        else
        {
            val = strtoull(val_as_str->string, &end, 10);
        }
    }

    return val;
}

//-------------------------------------------------------------------------
static uint32_t json_value_as_uint32(json_value_s* value, uint32_t default_value = 0)
{
    uint32_t val = 0;
    json_number_s* n = json_value_as_number(value);

    if (n != nullptr)
    {
        char* end = nullptr;
        val = strtoul(n->number, &end, 10);
    }
    else
    {
        // try to treat it as a string, then parse as long
        char* end = nullptr;
        json_string_s* val_as_str = json_value_as_string(value);

        if (val_as_str == nullptr || strlen(val_as_str->string) == 0)
        {
            val = default_value;
        }
        else
        {
            val = strtoul(val_as_str->string, &end, 10);
        }
    }

    return val;
}

static const char* pick_if_32bit_else(const char* choice_if_32bit, const char* choice_if_else)
{
#if PLATFORM_32BITS
    return choice_if_32bit;
#else
    return choice_if_else;
#endif
}

//-------------------------------------------------------------------------
static const char* eos_loglevel_to_print_str(EOS_ELogLevel level)
{
    switch (level)
    {
    case EOS_ELogLevel::EOS_LOG_Off:
        return "Off";
        break;
    case EOS_ELogLevel::EOS_LOG_Fatal:
        return "Fatal";
        break;
    case EOS_ELogLevel::EOS_LOG_Error:
        return "Error";
        break;
    case EOS_ELogLevel::EOS_LOG_Warning:
        return "Warning";
        break;
    case EOS_ELogLevel::EOS_LOG_Info:
        return "Info";
        break;
    case EOS_ELogLevel::EOS_LOG_Verbose:
        return "Verbose";
        break;
    case EOS_ELogLevel::EOS_LOG_VeryVerbose:
        return "VeryVerbose";
        break;
    default:
        return nullptr;
    }
}

//-------------------------------------------------------------------------
static void show_log_as_dialog(const char* log_string)
{
#if PLATFORM_WINDOWS
    MessageBoxA(NULL, log_string, "Warning", MB_ICONWARNING);
#endif
}

//-------------------------------------------------------------------------
static FILE* log_file_s = nullptr;
static std::vector<std::string> buffered_output;
void global_log_close()
{
    if (log_file_s)
    {
        fclose(log_file_s);
        log_file_s = nullptr;
        buffered_output.clear();
    }
}

//-------------------------------------------------------------------------
void global_logf(const char* format, ...)
{
    if (log_file_s != nullptr)
    {
        va_list arg_list;
        va_start(arg_list, format);
        vfprintf(log_file_s, format, arg_list);
        va_end(arg_list);

        fprintf(log_file_s, "\n");
        fflush(log_file_s);
    }
    else
    {
        va_list arg_list;
        va_start(arg_list, format);
        va_list arg_list_copy;
        va_copy(arg_list_copy, arg_list);
        const size_t printed_length = vsnprintf(nullptr, 0, format, arg_list) + 1;
        va_end(arg_list);

        std::vector<char> buffer(printed_length);
        vsnprintf(buffer.data(), printed_length, format, arg_list_copy);
        va_end(arg_list_copy);
        buffered_output.emplace_back(std::string(buffer.data(), printed_length));
    }
}

//-------------------------------------------------------------------------
void global_log_open(const char* filename)
{
    if (log_file_s != nullptr)
    {
        fclose(log_file_s);
        log_file_s = nullptr;
    }
    fopen_s(&log_file_s, filename, "w");

    if (buffered_output.size() > 0)
    {
        for (const std::string& str : buffered_output)
        {
            global_logf(str.c_str());
        }
        buffered_output.clear();
    }
}

typedef void (*log_flush_function_t)(const char* str);
DLL_EXPORT(void) global_log_flush_with_function(log_flush_function_t log_flush_function)
{
    if (buffered_output.size() > 0)
    {
        for (const std::string& str : buffered_output)
        {
            log_flush_function(str.c_str());
        }
        buffered_output.clear();
    }
}

//-------------------------------------------------------------------------
void log_base(const char* header, const char* message)
{
    constexpr size_t final_timestamp_len = 32;
    char final_timestamp[final_timestamp_len] = { };
    if (create_timestamp_str(final_timestamp, final_timestamp_len))
    {
        global_logf("%s NativePlugin (%s): %s", final_timestamp, header, message);
    }
    else
    {
        global_logf("NativePlugin (%s): %s", header, message);
    }
}

//-------------------------------------------------------------------------
// TODO: If possible, hook this up into a proper logging channel.s
void log_warn(const char* log_string)
{
#if SHOW_DIALOG_BOX_ON_WARN
    show_log_as_dialog(log_string);
#endif
    log_base("WARNING", log_string);
}

//-------------------------------------------------------------------------
void log_inform(const char* log_string)
{
    log_base("INFORM", log_string);
}

//-------------------------------------------------------------------------
void log_error(const char* log_string)
{
    log_base("ERROR", log_string);
}

//-------------------------------------------------------------------------
EXTERN_C void EOS_CALL eos_log_callback(const EOS_LogMessage* message)
{
    constexpr size_t final_timestamp_len = 32;
    char final_timestamp[final_timestamp_len] = {0};

    if (create_timestamp_str(final_timestamp, final_timestamp_len))
    {
        global_logf("%s %s (%s): %s", final_timestamp, message->Category, eos_loglevel_to_print_str(message->Level), message->Message);
    }
    else
    {
        global_logf("%s (%s): %s", message->Category, eos_loglevel_to_print_str(message->Level), message->Message);
    }

}

//-------------------------------------------------------------------------
static const char* null_if_empty(const std::string& str)
{
    return str.empty() ? nullptr : str.c_str();
}

//-------------------------------------------------------------------------
static TCHAR* get_path_to_module(HMODULE module)
{
    DWORD module_path_length = 128;
    TCHAR* module_path = (TCHAR*)malloc(module_path_length * sizeof(TCHAR));

    DWORD buffer_length = 0;
    DWORD GetModuleFileName_last_error = 0;

    do {
        buffer_length = GetModuleFileName(module, module_path, module_path_length);
        GetModuleFileName_last_error = GetLastError();
        SetLastError(NOERROR);

        if (GetModuleFileName_last_error == ERROR_INSUFFICIENT_BUFFER)
        {
            buffer_length = 0;
            module_path_length += 20;
            module_path = (TCHAR*)realloc(module_path, module_path_length * sizeof(TCHAR));
        }
    } while (buffer_length == 0);

    return module_path;
}

//-------------------------------------------------------------------------
static std::wstring get_path_to_module_as_string(HMODULE module)
{
    wchar_t* module_path = get_path_to_module(module);

    std::wstring module_file_path_string(module_path);
    free(module_path);
    return module_file_path_string;
}

//-------------------------------------------------------------------------
static fs::path get_path_relative_to_current_module(const fs::path& relative_path)
{
    HMODULE this_module = nullptr;
    if (!GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT, (LPCWSTR)&get_path_relative_to_current_module, &this_module) || !this_module)
    {
        return {};
    }

    std::wstring module_file_path_string = get_path_to_module_as_string(this_module);

    return fs::path(module_file_path_string).remove_filename() / relative_path;
}


//-------------------------------------------------------------------------
static void* load_library_at_path(const std::filesystem::path& library_path)
{
    void* to_return = nullptr;

#if PLATFORM_WINDOWS
    log_inform(("Loading path at " + to_utf8_str(library_path)).c_str());
    HMODULE handle = LoadLibrary(library_path.c_str());
    to_return = (void*)handle;
#endif

    return to_return;
}

//-------------------------------------------------------------------------
static void* load_function_with_name(void* library_handle, const char* function)
{
    void* to_return = nullptr;
#if PLATFORM_WINDOWS
    HMODULE handle = (HMODULE)library_handle;
    to_return = (void*)GetProcAddress(handle, function);
#endif
    return to_return;
}

//-------------------------------------------------------------------------
template<typename T>
T load_function_with_name(void* library_handle, const char* function)
{
    return reinterpret_cast<T>(load_function_with_name(library_handle, function));
}

//-------------------------------------------------------------------------
void unload_library(void* library_handle)
{
    FreeLibrary((HMODULE)library_handle);
}

//-------------------------------------------------------------------------
void eos_init(const EOSConfig& eos_config)
{
    static int reserved[2] = {1, 1};
    EOS_InitializeOptions SDKOptions = { 0 };
    SDKOptions.ApiVersion = EOS_INITIALIZE_API_LATEST;
    SDKOptions.AllocateMemoryFunction = nullptr;
    SDKOptions.ReallocateMemoryFunction = nullptr;
    SDKOptions.ReleaseMemoryFunction = nullptr;
    SDKOptions.ProductName = eos_config.productName.c_str();
    SDKOptions.ProductVersion = eos_config.productVersion.c_str();
    SDKOptions.Reserved = reserved;
    SDKOptions.SystemInitializeOptions = nullptr;

    EOS_Initialize_ThreadAffinity overrideThreadAffinity = {0};

    overrideThreadAffinity.ApiVersion = EOS_INITIALIZE_THREADAFFINITY_API_LATEST;

    overrideThreadAffinity.HttpRequestIo = eos_config.ThreadAffinity_HTTPRequestIO;
    overrideThreadAffinity.NetworkWork = eos_config.ThreadAffinity_networkWork;
    overrideThreadAffinity.P2PIo = eos_config.ThreadAffinity_P2PIO;
    overrideThreadAffinity.RTCIo = eos_config.ThreadAffinity_RTCIO;
    overrideThreadAffinity.StorageIo = eos_config.ThreadAffinity_storageIO;
    overrideThreadAffinity.WebSocketIo = eos_config.ThreadAffinity_webSocketIO;


    SDKOptions.OverrideThreadAffinity = &overrideThreadAffinity;

    log_inform("call EOS_Initialize");
    EOS_EResult InitResult = EOS_Initialize_ptr(&SDKOptions);
    if (InitResult != EOS_EResult::EOS_Success)
    {
        log_error("Unable to do eos init");
    }
    if (EOS_Logging_SetLogLevel_ptr != nullptr)
    {
        EOS_Logging_SetLogLevel_ptr(EOS_ELogCategory::EOS_LC_ALL_CATEGORIES, EOS_ELogLevel::EOS_LOG_VeryVerbose);
    }

    if (EOS_Logging_SetCallback_ptr != nullptr)
    {
        EOS_Logging_SetCallback_ptr(&eos_log_callback);
    }
}

//-------------------------------------------------------------------------
static char* GetCacheDirectory()
{
    static char* s_tempPathBuffer = NULL;

    if (s_tempPathBuffer == NULL)
    {
        WCHAR tmp_buffer = 0;
        DWORD buffer_size = GetTempPathW(1, &tmp_buffer) + 1;
        WCHAR* lpTempPathBuffer = (TCHAR*)malloc(buffer_size * sizeof(TCHAR));
        GetTempPathW(buffer_size, lpTempPathBuffer);

        s_tempPathBuffer = create_utf8_str_from_wide_str(lpTempPathBuffer);
        free(lpTempPathBuffer);
    }

    return s_tempPathBuffer;
}

//-------------------------------------------------------------------------
static json_value_s* read_config_json_as_json_from_path(std::filesystem::path path_to_config_json)
{
    log_inform(("json path" + to_utf8_str(path_to_config_json)).c_str());
    uintmax_t config_file_size = std::filesystem::file_size(path_to_config_json);
    if (config_file_size > SIZE_MAX)
    {
        throw std::filesystem::filesystem_error("File is too large", std::make_error_code(std::errc::file_too_large));
    }

    FILE* file = nullptr;
    errno_t config_file_error = _wfopen_s(&file, path_to_config_json.wstring().c_str(), L"r");
    char* buffer = (char*)calloc(1, static_cast<size_t>(config_file_size));

    size_t bytes_read = fread(buffer, 1, static_cast<size_t>(config_file_size), file);
    fclose(file);
    struct json_value_s* config_json = json_parse(buffer, bytes_read);
    free(buffer);

    return config_json;
}

//-------------------------------------------------------------------------
static json_value_s* read_config_json_from_dll()
{
    struct json_value_s* config_json = nullptr;

#if ENABLE_DLL_BASED_EOS_CONFIG
	log_inform("Trying to load eos config via dll");
    static void *eos_generated_library_handle = load_library_at_path(get_path_relative_to_current_module("EOSGenerated.dll"));

	if (!eos_generated_library_handle)
	{
		log_warn("No Generated DLL found (Might not be an error)");
		return NULL;
	}

    GetConfigAsJSONString = load_function_with_name<GetConfigAsJSONString_t>(eos_generated_library_handle, "GetConfigAsJSONString");
    
    if(GetConfigAsJSONString)
    {
        const char* config_as_json_string = GetConfigAsJSONString();
        if (config_as_json_string != nullptr)
        {
            size_t config_as_json_string_length = strlen(config_as_json_string);
            config_json = json_parse(config_as_json_string, config_as_json_string_length);
        }
    }
	else
	{
		log_warn("No function found");
	}
#endif

    return config_json;
}

//-------------------------------------------------------------------------
static EOSConfig eos_config_from_json_value(json_value_s* config_json)
{
    // Create platform instance
    struct json_object_s* config_json_object = json_value_as_object(config_json);
    struct json_object_element_s* iter = config_json_object->start;
    EOSConfig eos_config;

    while (iter != nullptr)
    {
        if (!strcmp("productName", iter->name->string))
        {
            eos_config.productName = json_value_as_string(iter->value)->string;
        }
        else if (!strcmp("productVersion", iter->name->string))
        {
            eos_config.productVersion = json_value_as_string(iter->value)->string;
        }
        else if (!strcmp("productID", iter->name->string))
        {
            eos_config.productID = json_value_as_string(iter->value)->string;
        }
        else if (!strcmp("sandboxID", iter->name->string))
        {
            eos_config.sandboxID = json_value_as_string(iter->value)->string;
        }
        else if (!strcmp("deploymentID", iter->name->string))
        {
            eos_config.deploymentID = json_value_as_string(iter->value)->string;
        }
        else if (!strcmp("sandboxDeploymentOverrides", iter->name->string))
        {
            json_array_s* overrides = json_value_as_array(iter->value);
            eos_config.sandboxDeploymentOverrides = std::vector<SandboxDeploymentOverride>();
            for (auto e = overrides->start; e != nullptr; e = e->next)
            {
                struct json_object_s* override_json_object = json_value_as_object(e->value);
                struct json_object_element_s* ov_iter = override_json_object->start;
                struct SandboxDeploymentOverride override_item = SandboxDeploymentOverride();
                while (ov_iter != nullptr)
                {
                    if (!strcmp("sandboxID", ov_iter->name->string))
                    {
                        override_item.sandboxID = json_value_as_string(ov_iter->value)->string;
                    }
                    else if (!strcmp("deploymentID", ov_iter->name->string))
                    {
                        override_item.deploymentID = json_value_as_string(ov_iter->value)->string;
                    }
                    ov_iter = ov_iter->next;
                }
                eos_config.sandboxDeploymentOverrides.push_back(override_item);
            }
        }
        else if (!strcmp("clientID", iter->name->string))
        {
            eos_config.clientID = json_value_as_string(iter->value)->string;
        }
        else if (!strcmp("clientSecret", iter->name->string))
        {
            eos_config.clientSecret = json_value_as_string(iter->value)->string;
        }
        if (!strcmp("encryptionKey", iter->name->string))
        {
            eos_config.encryptionKey = json_value_as_string(iter->value)->string;
        }
        else if (!strcmp("overrideCountryCode ", iter->name->string))
        {
            eos_config.overrideCountryCode = json_value_as_string(iter->value)->string;
        }
        else if (!strcmp("overrideLocaleCode", iter->name->string))
        {
            eos_config.overrideLocaleCode = json_value_as_string(iter->value)->string;
        }
        else if (!strcmp("platformOptionsFlags", iter->name->string))
        {
            uint64_t collected_flags = 0;
            json_array_s* flags = json_value_as_array(iter->value);
            for (auto e = flags->start; e != nullptr; e = e->next)
            {
                const char* flag_as_cstr = json_value_as_string(e->value)->string;

                if (!strcmp("EOS_PF_LOADING_IN_EDITOR", flag_as_cstr) || !strcmp("LoadingInEditor", flag_as_cstr))
                {
                    collected_flags |= EOS_PF_LOADING_IN_EDITOR;
                }

                if (!strcmp("EOS_PF_DISABLE_OVERLAY", flag_as_cstr) || !strcmp("DisableOverlay", flag_as_cstr))
                {
                    collected_flags |= EOS_PF_DISABLE_OVERLAY;
                }

                if (!strcmp("EOS_PF_DISABLE_SOCIAL_OVERLAY", flag_as_cstr) || !strcmp("DisableSocialOverlay", flag_as_cstr))
                {
                    collected_flags |= EOS_PF_DISABLE_SOCIAL_OVERLAY;
                }

                if (!strcmp("EOS_PF_RESERVED1", flag_as_cstr) || !strcmp("Reserved1", flag_as_cstr))
                {
                    collected_flags |= EOS_PF_RESERVED1;
                }

                if (!strcmp("EOS_PF_WINDOWS_ENABLE_OVERLAY_D3D9", flag_as_cstr) || !strcmp("WindowsEnableOverlayD3D9", flag_as_cstr))
                {
                    collected_flags |= EOS_PF_WINDOWS_ENABLE_OVERLAY_D3D9;
                }

                if (!strcmp("EOS_PF_WINDOWS_ENABLE_OVERLAY_D3D10", flag_as_cstr) || !strcmp("WindowsEnableOverlayD3D10", flag_as_cstr))
                {
                    collected_flags |= EOS_PF_WINDOWS_ENABLE_OVERLAY_D3D10;
                }

                if (!strcmp("EOS_PF_WINDOWS_ENABLE_OVERLAY_OPENGL", flag_as_cstr) || !strcmp("WindowsEnableOverlayOpengl", flag_as_cstr))
                {
                    collected_flags |= EOS_PF_WINDOWS_ENABLE_OVERLAY_OPENGL;
                }
            }

            eos_config.flags = collected_flags;
        }
        else if (!strcmp("tickBudgetInMilliseconds", iter->name->string))
        {
            eos_config.tickBudgetInMilliseconds = json_value_as_uint32(iter->value);
        }
        else if (!strcmp("ThreadAffinity_networkWork", iter->name->string))
        {
            eos_config.ThreadAffinity_networkWork = json_value_as_uint64(iter->value);
        }
        else if (!strcmp("ThreadAffinity_storageIO", iter->name->string))
        {
            eos_config.ThreadAffinity_storageIO = json_value_as_uint64(iter->value);
        }
        else if (!strcmp("ThreadAffinity_webSocketIO", iter->name->string))
        {
            eos_config.ThreadAffinity_webSocketIO = json_value_as_uint64(iter->value);
        }
        else if (!strcmp("ThreadAffinity_P2PIO", iter->name->string))
        {
            eos_config.ThreadAffinity_P2PIO = json_value_as_uint64(iter->value);
        }
        else if (!strcmp("ThreadAffinity_HTTPRequestIO", iter->name->string))
        {
            eos_config.ThreadAffinity_HTTPRequestIO = json_value_as_uint64(iter->value);
        }
        else if (!strcmp("ThreadAffinity_RTCIO", iter->name->string))
        {
            eos_config.ThreadAffinity_RTCIO = json_value_as_uint64(iter->value);
        }
        else if (!strcmp("isServer", iter->name->string))
        {
            // In this JSON library, true and false are _technically_ different types. 
            if (json_value_is_true(iter->value))
            {
                eos_config.isServer = true;
            }
            else if (json_value_is_false(iter->value))
            {
                eos_config.isServer = false;
            }
        }

        iter = iter->next;
    }
    
    return eos_config;
}

//-------------------------------------------------------------------------
static bool str_is_equal_to_any(const char* str, ...)
{
    bool to_return = false;
    va_list arg_list;
    va_start(arg_list, str);

    const char *value = va_arg(arg_list, const char*);

    while (value != NULL)
    {
        if (!strcmp(str, value))
        {
            to_return = true;
            break;
        }
        value = va_arg(arg_list, const char*);
    }

    va_end(arg_list);

    return to_return;
}

//-------------------------------------------------------------------------
static bool str_is_equal_to_none(const char* str, ...)
{
    bool to_return = false;
    va_list arg_list;
    va_start(arg_list, str);

    const char *value = va_arg(arg_list, const char*);

    while (value != NULL)
    {
        if (strcmp(str, value))
        {
            to_return = true;
            break;
        }
        value = va_arg(arg_list, const char*);
    }

    va_end(arg_list);

    return to_return;
}

//-------------------------------------------------------------------------
static EOS_EIntegratedPlatformManagementFlags eos_collect_integrated_platform_managment_flags(json_object_element_s* iter)
{
    EOS_EIntegratedPlatformManagementFlags collected_flags = static_cast<EOS_EIntegratedPlatformManagementFlags>(0);
    json_array_s* flags = json_value_as_array(iter->value);
    bool flag_set = false;
    for (auto e = flags->start; e != nullptr; e = e->next)
    {
        const char* flag_as_cstr = json_value_as_string(e->value)->string;

        if (str_is_equal_to_any(flag_as_cstr, "EOS_IPMF_Disabled", "Disabled", NULL))
        {
            collected_flags |= EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_Disabled;
            flag_set = true;
        }

        else if (str_is_equal_to_any(flag_as_cstr, "EOS_IPMF_ManagedByApplication", "ManagedByApplication", "EOS_IPMF_LibraryManagedByApplication", NULL))
        {
            collected_flags |= EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_LibraryManagedByApplication;
            flag_set = true;
        }
        else if (str_is_equal_to_any(flag_as_cstr,"EOS_IPMF_ManagedBySDK", "ManagedBySDK", "EOS_IPMF_LibraryManagedBySDK", NULL))
        {
            collected_flags |= EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_LibraryManagedBySDK;
            flag_set = true;
        }
        else if (str_is_equal_to_any(flag_as_cstr, "EOS_IPMF_DisableSharedPresence", "DisableSharedPresence", "EOS_IPMF_DisablePresenceMirroring", NULL))
        {
            collected_flags |= EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_DisablePresenceMirroring;
            flag_set = true;
        }
        else if (str_is_equal_to_any(flag_as_cstr, "EOS_IPMF_DisableSessions", "DisableSessions", "EOS_IPMF_DisableSDKManagedSessions", NULL))
        {
            collected_flags |= EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_DisableSDKManagedSessions;
            flag_set = true;
        }
        else if (str_is_equal_to_any(flag_as_cstr, "EOS_IPMF_PreferEOS", "PreferEOS", "EOS_IPMF_PreferEOSIdentity", NULL))
        {
            collected_flags |= EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_PreferEOSIdentity;
            flag_set = true;
        }
        else if (str_is_equal_to_any(flag_as_cstr, "EOS_IPMF_PreferIntegrated", "PreferIntegrated", "EOS_IPMF_PreferIntegratedIdentity", NULL))
        {
            collected_flags |= EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_PreferIntegratedIdentity;
            flag_set = true;
        }
    }

    return flag_set ? collected_flags : EOS_EIntegratedPlatformManagementFlags::EOS_IPMF_Disabled;
}

//-------------------------------------------------------------------------
static EOSSteamConfig eos_steam_config_from_json_value(json_value_s *config_json)
{
    struct json_object_s* config_json_object = json_value_as_object(config_json);
    struct json_object_element_s* iter = config_json_object->start;
    EOSSteamConfig eos_config;
    eos_config.flags;

    while (iter != nullptr)
    {
        if (!strcmp("flags", iter->name->string))
        {
            eos_config.flags = eos_collect_integrated_platform_managment_flags(iter);

        }
        else if (!strcmp("overrideLibraryPath", iter->name->string))
        {
            const char *override_library_path = json_value_as_string(iter->value)->string;

            if (strcmp("NULL", override_library_path)
                && strcmp("null", override_library_path)
                )
            {
                eos_config.OverrideLibraryPath = override_library_path;
            }

        }
        else if (!strcmp("steamSDKMajorVersion", iter->name->string))
        {
            eos_config.steamSDKMajorVersion = json_value_as_uint32(iter->value);
        }
        else if (!strcmp("steamSDKMinorVersion", iter->name->string))
        {
            eos_config.steamSDKMinorVersion = json_value_as_uint32(iter->value);
        }
        iter = iter->next;
    }

    return eos_config;
}

//-------------------------------------------------------------------------
static std::filesystem::path get_path_for_eos_service_config(std::string config_filename)
{
    //return get_path_relative_to_current_module(std::filesystem::path("../..") / "StreamingAssets" / "EOS" / "EpicOnlineServicesConfig.json");
	auto twoDirsUp = std::filesystem::path("../..");
	std::filesystem::path packaged_data_path = get_path_relative_to_current_module(twoDirsUp);
	std::error_code error_code;

	log_inform("about to look with exists");
	if (!std::filesystem::exists(packaged_data_path, error_code))
	{
		log_warn("Didn't find the path twoDirsUp");
		packaged_data_path = get_path_relative_to_current_module(std::filesystem::path("./Data/"));
	}
	
	return packaged_data_path / "StreamingAssets" / "EOS" / config_filename;
}

//-------------------------------------------------------------------------
json_value_s* read_eos_config_as_json_value_from_file(std::string config_filename)
{
    std::filesystem::path path_to_config_json = get_path_for_eos_service_config(config_filename);

    return read_config_json_as_json_from_path(path_to_config_json);
}

//-------------------------------------------------------------------------
static void EOS_Platform_Options_debug_log(const EOS_Platform_Options& platform_options)
{
    std::stringstream output;
    output << platform_options.ApiVersion << "\n";
    output << platform_options.bIsServer << "\n";
    output << platform_options.Flags << "\n";
    output << platform_options.CacheDirectory << "\n";

    output << platform_options.EncryptionKey << "\n";
    if (platform_options.OverrideCountryCode)
    {
        output << platform_options.OverrideCountryCode << "\n";
    }

    if (platform_options.OverrideLocaleCode)
    {
        output << platform_options.OverrideLocaleCode << "\n";
    }
    output << platform_options.ProductId << "\n";
    output << platform_options.SandboxId << "\n";
    output << platform_options.DeploymentId << "\n";
    output << platform_options.ClientCredentials.ClientId << "\n";
    output << platform_options.ClientCredentials.ClientSecret << "\n";

    auto *rtc_options = platform_options.RTCOptions;
    auto *windows_rtc_options = (EOS_Windows_RTCOptions*)rtc_options->PlatformSpecificOptions;

    output << windows_rtc_options->ApiVersion << "\n";
    output << windows_rtc_options->XAudio29DllPath << "\n";

    log_inform(output.str().c_str());
}

//-------------------------------------------------------------------------
static std::string basename(const std::string& path)
{
    std::string filename;
    filename.resize(path.length() + 1);
    _splitpath_s(path.c_str(), NULL, 0, NULL, 0, filename.data(), filename.size(), NULL, 0);

    return filename;
}

//-------------------------------------------------------------------------
static void eos_call_steam_init(const std::filesystem::path& steam_dll_path)
{
    std::string steam_dll_path_as_string = steam_dll_path.string();
    eos_call_steam_init(steam_dll_path_as_string);
}

//-------------------------------------------------------------------------
// This function assumes that if the caller has already loaded the steam DLL,
// that SteamAPI_Init doesn't need to be called
static void eos_call_steam_init(const std::string& steam_dll_path)
{
    auto steam_dll_path_string = basename(steam_dll_path);
    HANDLE steam_dll_handle = GetModuleHandleA(steam_dll_path_string.c_str());

    // Check the default name for the steam_api.dll
    if (!steam_dll_handle)
    {
        steam_dll_handle = GetModuleHandleA("steam_api.dll");
    }

    // in the case that it's not loaded, try to load it from the user provided path
    if (!steam_dll_handle)
    {
        steam_dll_handle = load_library_at_path(steam_dll_path);
    }

    if (steam_dll_handle != nullptr)
    {
        typedef bool(__cdecl* SteamAPI_Init_t)();
        SteamAPI_Init_t SteamAPI_Init = load_function_with_name<SteamAPI_Init_t>(steam_dll_handle, "SteamAPI_Init");

        if (SteamAPI_Init())
        {
            log_inform("Called SteamAPI_Init with success!");
        }
    }
}

//-------------------------------------------------------------------------
void eos_create(EOSConfig& eosConfig)
{
    EOS_Platform_Options platform_options = {0};
    platform_options.ApiVersion = EOS_PLATFORM_OPTIONS_API_LATEST;
    platform_options.bIsServer = eosConfig.isServer;
    platform_options.Flags = eosConfig.flags;
    platform_options.CacheDirectory = GetCacheDirectory();

    platform_options.EncryptionKey = eosConfig.encryptionKey.length() > 0 ? eosConfig.encryptionKey.c_str() : nullptr;
    platform_options.OverrideCountryCode = eosConfig.overrideCountryCode.length() > 0 ? eosConfig.overrideCountryCode.c_str() : nullptr;
    platform_options.OverrideLocaleCode = eosConfig.overrideLocaleCode.length() > 0 ? eosConfig.overrideLocaleCode.c_str() : nullptr;
    platform_options.ProductId = eosConfig.productID.c_str();
    platform_options.SandboxId = eosConfig.sandboxID.c_str();
    platform_options.DeploymentId = eosConfig.deploymentID.c_str();
    platform_options.ClientCredentials.ClientId = eosConfig.clientID.c_str();
    platform_options.ClientCredentials.ClientSecret = eosConfig.clientSecret.c_str();

    platform_options.TickBudgetInMilliseconds = eosConfig.tickBudgetInMilliseconds;

    EOS_Platform_RTCOptions rtc_options = { 0 };

    rtc_options.ApiVersion = EOS_PLATFORM_RTCOPTIONS_API_LATEST;
#if PLATFORM_WINDOWS
    log_inform("setting up rtc");
    fs::path xaudio2_dll_path = get_path_relative_to_current_module(XAUDIO2_DLL_NAME);
    std::string xaudio2_dll_path_as_string = to_utf8_str(xaudio2_dll_path);
    EOS_Windows_RTCOptions windows_rtc_options = { 0 };
    windows_rtc_options.ApiVersion = EOS_WINDOWS_RTCOPTIONS_API_LATEST;
    windows_rtc_options.XAudio29DllPath = xaudio2_dll_path_as_string.c_str();
    log_warn(xaudio2_dll_path_as_string.c_str());

    if (!fs::exists(xaudio2_dll_path))
    {
        log_warn("Missing XAudio dll!");
    }
    rtc_options.PlatformSpecificOptions = &windows_rtc_options;
    platform_options.RTCOptions = &rtc_options;
#endif

#if PLATFORM_WINDOWS
    auto path_to_steam_config_json = get_path_for_eos_service_config(EOS_STEAM_CONFIG_FILENAME);

    // Defined here so that the override path lives long enough to be referenced by the create option
    EOSSteamConfig eos_steam_config;
    EOS_IntegratedPlatform_Options steam_integrated_platform_option = { 0 };
    EOS_IntegratedPlatform_Steam_Options steam_platform = { 0 };
    EOS_HIntegratedPlatformOptionsContainer integrated_platform_options_container = nullptr;
    std::wstring_convert<std::codecvt_utf8<wchar_t>> converter;

    if (std::filesystem::exists(path_to_steam_config_json))
    {
        json_value_s* eos_steam_config_as_json = nullptr;
        eos_steam_config_as_json = read_config_json_as_json_from_path(path_to_steam_config_json);
        eos_steam_config = eos_steam_config_from_json_value(eos_steam_config_as_json);
        free(eos_steam_config_as_json);

        if (eos_steam_config.OverrideLibraryPath.has_value())
        {
            if (!std::filesystem::exists(eos_steam_config.OverrideLibraryPath.value()))
            {
                auto override_lib_path_as_str = basename(eos_steam_config.OverrideLibraryPath.value());
                auto found_steam_path = get_path_relative_to_current_module(override_lib_path_as_str);

                // Fall back and use the steam dll name based on the
                // type of binary the GfxPluginNativeRender
                if (!std::filesystem::exists(found_steam_path) || eos_steam_config.OverrideLibraryPath.value().empty())
                {
                    found_steam_path = get_path_relative_to_current_module(STEAM_DLL_NAME);
                }

                if (std::filesystem::exists(found_steam_path))
                {
                    eos_steam_config.OverrideLibraryPath = converter.to_bytes(found_steam_path.wstring());
                }
            }
        }
        else
        {
            auto found_steam_path = get_path_relative_to_current_module(STEAM_DLL_NAME);
            if (std::filesystem::exists(found_steam_path))
            {
                eos_steam_config.OverrideLibraryPath = converter.to_bytes(found_steam_path.wstring());
            }
        }

        if (eos_steam_config.isManagedByApplication())
        {
            eos_call_steam_init(eos_steam_config.OverrideLibraryPath.value());
            eos_steam_config.OverrideLibraryPath.reset();
        }

        if (eos_steam_config.OverrideLibraryPath.has_value())
        {
            steam_platform.OverrideLibraryPath = eos_steam_config.OverrideLibraryPath.value().c_str();
        }


        steam_platform.SteamMajorVersion = eos_steam_config.steamSDKMajorVersion;
        steam_platform.SteamMinorVersion = eos_steam_config.steamSDKMinorVersion;

        steam_integrated_platform_option.ApiVersion = EOS_INTEGRATEDPLATFORM_OPTIONS_API_LATEST;
        steam_integrated_platform_option.Type = EOS_IPT_Steam;
        steam_integrated_platform_option.Flags = eos_steam_config.flags;
        steam_integrated_platform_option.InitOptions = &steam_platform;

        steam_platform.ApiVersion = EOS_INTEGRATEDPLATFORM_STEAM_OPTIONS_API_LATEST;

        EOS_IntegratedPlatform_CreateIntegratedPlatformOptionsContainerOptions options = { EOS_INTEGRATEDPLATFORM_CREATEINTEGRATEDPLATFORMOPTIONSCONTAINER_API_LATEST };
        EOS_IntegratedPlatform_CreateIntegratedPlatformOptionsContainer_ptr(&options, &integrated_platform_options_container);
        platform_options.IntegratedPlatformOptionsContainerHandle = integrated_platform_options_container;

        EOS_IntegratedPlatformOptionsContainer_AddOptions addOptions = { EOS_INTEGRATEDPLATFORMOPTIONSCONTAINER_ADD_API_LATEST };
        addOptions.Options = &steam_integrated_platform_option;
        EOS_IntegratedPlatformOptionsContainer_Add_ptr(integrated_platform_options_container, &addOptions);
    }
#endif

    //EOS_Platform_Options_debug_log(platform_options);
    log_inform("run EOS_Platform_Create");
    eos_platform_handle = EOS_Platform_Create_ptr(&platform_options);
    if (integrated_platform_options_container)
    {
        EOS_IntegratedPlatformOptionsContainer_Release_ptr(integrated_platform_options_container);
    }

    if (!eos_platform_handle)
    {
        log_error("failed to create the platform");
    }
}

//-------------------------------------------------------------------------
static bool QueryRegKey(const HKEY InKey, const TCHAR* InSubKey, const TCHAR* InValueName, std::wstring& OutData)
{
    bool bSuccess = false;
#if PLATFORM_WINDOWS
    // Redirect key depending on system
    for (uint32_t RegistryIndex = 0; RegistryIndex < 2 && !bSuccess; ++RegistryIndex)
    {
        HKEY Key = 0;
        const uint32_t RegFlags = (RegistryIndex == 0) ? KEY_WOW64_32KEY : KEY_WOW64_64KEY;
        if (RegOpenKeyEx(InKey, InSubKey, 0, KEY_READ | RegFlags, &Key) == ERROR_SUCCESS)
        {
            ::DWORD Size = 0;
            // First, we'll call RegQueryValueEx to find out how large of a buffer we need
            if ((RegQueryValueEx(Key, InValueName, NULL, NULL, NULL, &Size) == ERROR_SUCCESS) && Size)
            {
                // Allocate a buffer to hold the value and call the function again to get the data
                char *Buffer = new char[Size];
                if (RegQueryValueEx(Key, InValueName, NULL, NULL, (LPBYTE)Buffer, &Size) == ERROR_SUCCESS)
                {
                    const uint32_t Length = (Size / sizeof(TCHAR)) - 1;
                    OutData = (TCHAR*)Buffer;
                    bSuccess = true;
                }
                delete[] Buffer;
            }
            RegCloseKey(Key);
        }
    }
#endif
    return bSuccess;
}

//-------------------------------------------------------------------------
// Currently this only works on windows
static bool get_overlay_dll_path(fs::path* OutDllPath)
{
#if PLATFORM_WINDOWS
    const TCHAR* RegKey = TEXT(R"(SOFTWARE\Epic Games\EOS)");
    const TCHAR* RegValue = TEXT("OverlayPath");
    std::wstring OverlayDllDirectory;

    if (!QueryRegKey(HKEY_CURRENT_USER, RegKey, RegValue, OverlayDllDirectory))
    {
        if (!QueryRegKey(HKEY_LOCAL_MACHINE, RegKey, RegValue, OverlayDllDirectory))
        {
            return false;
        }
    }

    *OutDllPath = fs::path(OverlayDllDirectory) / OVERLAY_DLL_NAME;
    return fs::exists(*OutDllPath) && fs::is_regular_file(*OutDllPath);
#else
    log_inform("Trying to get a DLL path on a platform without DLL paths searching");
    return false;
#endif
}

//-------------------------------------------------------------------------
static void FetchEOSFunctionPointers()
{
    // The '@' in the function names is apart of how names are mangled on windows. The value after the '@' is the size of the params on the stack
    EOS_Initialize_ptr = load_function_with_name<EOS_Initialize_t>(s_eos_sdk_lib_handle, pick_if_32bit_else("_EOS_Initialize@4", "EOS_Initialize"));
    EOS_Shutdown_ptr = load_function_with_name<EOS_Shutdown_t>(s_eos_sdk_lib_handle, pick_if_32bit_else("_EOS_Shutdown@0", "EOS_Shutdown"));
    EOS_Platform_Create_ptr = load_function_with_name<EOS_Platform_Create_t>(s_eos_sdk_lib_handle, pick_if_32bit_else("_EOS_Platform_Create@4", "EOS_Platform_Create"));
    EOS_Platform_Release_ptr = load_function_with_name<EOS_Platform_Release_t>(s_eos_sdk_lib_handle, pick_if_32bit_else("_EOS_Platform_Release@4", "EOS_Platform_Release"));
    EOS_Logging_SetLogLevel_ptr = load_function_with_name<EOS_Logging_SetLogLevel_t>(s_eos_sdk_lib_handle, pick_if_32bit_else("_EOS_Logging_SetLogLevel@8", "EOS_Logging_SetLogLevel"));
    EOS_Logging_SetCallback_ptr = load_function_with_name<EOS_Logging_SetCallback_t>(s_eos_sdk_lib_handle, pick_if_32bit_else("EOS_Logging_SetCallback@4", "EOS_Logging_SetCallback"));

    EOS_IntegratedPlatformOptionsContainer_Add_ptr = load_function_with_name<EOS_IntegratedPlatformOptionsContainer_Add_t>(s_eos_sdk_lib_handle, pick_if_32bit_else("_EOS_IntegratedPlatformOptionsContainer_Add@8", "EOS_IntegratedPlatformOptionsContainer_Add"));
    EOS_IntegratedPlatform_CreateIntegratedPlatformOptionsContainer_ptr = load_function_with_name<EOS_IntegratedPlatform_CreateIntegratedPlatformOptionsContainer_t>(s_eos_sdk_lib_handle, pick_if_32bit_else("_EOS_IntegratedPlatform_CreateIntegratedPlatformOptionsContainer@8", "EOS_IntegratedPlatform_CreateIntegratedPlatformOptionsContainer"));
    EOS_IntegratedPlatformOptionsContainer_Release_ptr = load_function_with_name<EOS_IntegratedPlatformOptionsContainer_Release_t>(s_eos_sdk_lib_handle, pick_if_32bit_else("_EOS_IntegratedPlatformOptionsContainer_Release@4", "EOS_IntegratedPlatformOptionsContainer_Release"));
}

//-------------------------------------------------------------------------
// Called by unity on load. It kicks off the work to load the DLL for Overlay
#if PLATFORM_32BITS
#pragma comment(linker, "/export:UnityPluginLoad=_UnityPluginLoad@4")
#endif
DLL_EXPORT(void) UnityPluginLoad(void*)
{
#if _DEBUG
    show_log_as_dialog("You may attach a debugger to the DLL");
#endif

    auto path_to_config_json = get_path_for_eos_service_config(EOS_SERVICE_CONFIG_FILENAME);
    json_value_s* eos_config_as_json = nullptr;

    eos_config_as_json = read_config_json_from_dll();

    if (!eos_config_as_json && std::filesystem::exists(path_to_config_json))
    {
        eos_config_as_json = read_config_json_as_json_from_path(path_to_config_json);
    }

    if (!eos_config_as_json)
    {
        log_warn("Failed to load a valid json config for EOS");
        return;
    }

    EOSConfig eos_config = eos_config_from_json_value(eos_config_as_json);
    free(eos_config_as_json);

#if PLATFORM_WINDOWS
    //support sandbox and deployment id override via command line arguments
    std::stringstream argStream = std::stringstream(GetCommandLineA());
    std::istream_iterator<std::string> argsBegin(argStream);
    std::istream_iterator<std::string> argsEnd;
    std::vector<std::string> argStrings(argsBegin, argsEnd);
    std::string egsArgName = "-epicsandboxid=";
    std::string sandboxArgName = "-eossandboxid=";
    for (unsigned i = 0; i < argStrings.size(); ++i)
    {
        std::string* match = nullptr;
        if (argStrings[i]._Starts_with(sandboxArgName))
        {
            match = &sandboxArgName;
        }
        else if(argStrings[i]._Starts_with(egsArgName))
        {
            match = &egsArgName;
        }
        if (match != nullptr)
        {
            std::string sandboxArg = argStrings[i].substr(match->length());
            if (!sandboxArg.empty())
            {
                log_inform(("Sandbox ID override specified: " + sandboxArg).c_str());
                eos_config.sandboxID = sandboxArg;
            }
        }
    }
#endif

    //check if a deployment id override exists for sandbox id
    for (unsigned i = 0; i < eos_config.sandboxDeploymentOverrides.size(); ++i)
    {
        if (eos_config.sandboxID == eos_config.sandboxDeploymentOverrides[i].sandboxID)
        {
            log_inform(("Sandbox Deployment ID override specified: " + eos_config.sandboxDeploymentOverrides[i].deploymentID).c_str());
            eos_config.deploymentID = eos_config.sandboxDeploymentOverrides[i].deploymentID;
        }
    }

#if PLATFORM_WINDOWS
    std::string deploymentArgName = "-eosdeploymentid=";
    for (unsigned i = 0; i < argStrings.size(); ++i)
    {
        if (argStrings[i]._Starts_with(deploymentArgName))
        {
            std::string deploymentArg = argStrings[i].substr(deploymentArgName.length());
            if (!deploymentArg.empty())
            {
                log_inform(("Deployment ID override specified: " + deploymentArg).c_str());
                eos_config.deploymentID = deploymentArg;
            }
        }
    }
#endif

#if _DEBUG
    global_log_open("gfx_log.txt");
#endif

    fs::path DllPath;
    log_inform("On UnityPluginLoad");
    //if (!get_overlay_dll_path(&DllPath))
    //{
    //    show_log_as_dialog("Missing Overlay DLL!\n Overlay functionality will not work!");
    //}

    s_eos_sdk_lib_handle = load_library_at_path(get_path_relative_to_current_module(SDK_DLL_NAME));

    //eos_sdk_overlay_lib_handle = load_library_at_path(DllPath);
    //if (eos_sdk_overlay_lib_handle)
    //{
    //    log_warn("loaded eos overlay sdk");
    //    FuncApplicationWillShutdown = load_function_with_name<FSig_ApplicationWillShutdown>(eos_sdk_overlay_lib_handle, "EOS_Overlay_Initilize");
    //    if(FuncApplicationWillShutdown == nullptr)
    //    {
    //        log_warn("Unable to find overlay function");
    //    }
    //}

    if (s_eos_sdk_lib_handle)
    {
        FetchEOSFunctionPointers();

        if (EOS_Initialize_ptr)
        {
            log_inform("start eos init");

            eos_init(eos_config);

            //log_warn("start eos create");
            eos_create(eos_config);

            // This code is commented out because the handle is now handed off to the C# code
            //EOS_Platform_Release(eos_platform_handle);
            //eos_platform_handle = NULL;
            //log_warn("start eos shutdown");
            //EOS_Shutdown();
            //log_warn("unload eos sdk");
            //unload_library(s_eos_sdk_lib_handle);

            s_eos_sdk_lib_handle = NULL;
            EOS_Initialize_ptr = NULL;
            EOS_Shutdown_ptr = NULL;
            EOS_Platform_Create_ptr = NULL;
        }
        else
        {
            log_warn("unable to find EOS_Initialize");
        }

    }
    else
    {
        log_warn("Couldn't find dll "  SDK_DLL_NAME);
    }

}

//-------------------------------------------------------------------------
#if PLATFORM_32BITS
#pragma comment(linker, "/export:_UnityPluginUnload=_UnityPluginUnload@0")
#endif
DLL_EXPORT(void) UnityPluginUnload()
{
    if (FuncApplicationWillShutdown != nullptr)
    {
        FuncApplicationWillShutdown();
    }
    unload_library(s_eos_sdk_overlay_lib_handle);
    s_eos_sdk_overlay_lib_handle = nullptr;

    global_log_close();
}

//-------------------------------------------------------------------------
DLL_EXPORT(void) UnloadEOS()
{
    if (EOS_Shutdown_ptr)
    {
        log_inform("EOS shutdown");
        EOS_Shutdown_ptr();
    }
    if (s_eos_sdk_lib_handle)
    {
        log_inform("Unload eos sdk handle");
        unload_library(s_eos_sdk_lib_handle);
    }
}

//-------------------------------------------------------------------------
DLL_EXPORT(void *) EOS_GetPlatformInterface()
{
    return eos_platform_handle;
}
