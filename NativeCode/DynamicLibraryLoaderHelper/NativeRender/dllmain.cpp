// dllmain.cpp : Defines the entry point for the DLL application.
// This file does some *magick* to load the EOS Overlay DLL.
// This is apparently needed so that the Overlay can render properly
#include "pch.h"
#include <stdio.h>
#include <assert.h>
#include <stdlib.h>

#include <string>
#include <functional>
#include <algorithm>
#include <utility>
#include <filesystem>


//#include "eos_minimum_includes.h"
#include "Windows/eos_Windows_base.h"
#include "eos_sdk.h"

#include "json.h"

#if PLATFORM_64BITS
#define DLL_PLATFORM "-Win64"
#else
#define DLL_PLATFORM "-Win32"
#endif

#define DLL_SUFFIX "-Shipping.dll"

#define SHOW_DIALOG_BOX_ON_WARN 0
#define ENABLE_DLL_BASED_EOS_CONFIG 1
#define OVERLAY_DLL_NAME "EOSOVH" DLL_PLATFORM DLL_SUFFIX
#define SDK_DLL_NAME "EOSSDK" DLL_PLATFORM DLL_SUFFIX

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

static EOS_Initialize_t EOS_Initialize_ptr;
static EOS_Shutdown_t EOS_Shutdown_ptr;
static EOS_Platform_Create_t EOS_Platform_Create_ptr;
static EOS_Platform_Release_t EOS_Platform_Release_ptr;

static void *s_eos_sdk_overlay_lib_handle;
static void *s_eos_sdk_lib_handle;
static EOS_HPlatform eos_platform_handle;
static GetConfigAsJSONString_t GetConfigAsJSONString;

struct EOSConfig
{
    std::string productName;
    std::string productVersion;

    std::string productID;
    std::string sandboxID;
    std::string deploymentID;

    std::string clientSecret;
    std::string clientID;
    std::string encryptionKey;

    std::string overrideCountryCode;
    std::string overrideLocaleCode;
};

extern "C"
{
    void __declspec(dllexport) __stdcall UnityPluginLoad(void* unityInterfaces);
    void __declspec(dllexport) __stdcall UnityPluginUnload();

    void __declspec(dllexport) __stdcall UnloadEOS();
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
// TODO: If possible, hook this up into a proper logging channel.s
void log_warn(const char* log_string)
{
#if SHOW_DIALOG_BOX_ON_WARN
#if PLATFORM_WINDOWS
    MessageBoxA(NULL, log_string, "Warning", MB_ICONWARNING);
#endif
#endif

    printf("WARNING: %s\n", log_string);
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
	log_warn(("Loading path at " + library_path.string()).c_str());
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
    EOS_InitializeOptions SDKOptions = { 0 };
    SDKOptions.ApiVersion = EOS_INITIALIZE_API_LATEST;
    SDKOptions.AllocateMemoryFunction = nullptr;
    SDKOptions.ReallocateMemoryFunction = nullptr;
    SDKOptions.ReleaseMemoryFunction = nullptr;
	SDKOptions.ProductName = eos_config.productName.c_str();
	SDKOptions.ProductVersion = eos_config.productVersion.c_str();
    SDKOptions.Reserved = nullptr;
    SDKOptions.SystemInitializeOptions = nullptr;
    SDKOptions.OverrideThreadAffinity = nullptr;

    log_warn("call EOS_Initialize");
    EOS_EResult InitResult = EOS_Initialize_ptr(&SDKOptions);
    if (InitResult != EOS_EResult::EOS_Success)
    {
        log_warn("Unable to do eos init");
    }
}

//-------------------------------------------------------------------------
static char* GetCacheDirectory()
{
	static char* lpTempPathBuffer = NULL;

	if (lpTempPathBuffer == NULL)
	{
		char tmp_buffer = 0;
		DWORD buffer_size = GetTempPathA(1, &tmp_buffer) + 1;
		lpTempPathBuffer = (char*)malloc(buffer_size);
		GetTempPathA(buffer_size, lpTempPathBuffer);
	}
    return lpTempPathBuffer;
}

//-------------------------------------------------------------------------
static json_value_s* read_config_json_as_json_from_path(std::filesystem::path path_to_config_json)
{
	log_warn(("json path" + path_to_config_json.string() ).c_str());
	uintmax_t config_file_size = std::filesystem::file_size(path_to_config_json);

	FILE* file = nullptr;
	errno_t config_file_error = _wfopen_s(&file, path_to_config_json.wstring().c_str(), L"r");
	char* buffer = (char*)calloc(1, config_file_size);

	size_t bytes_read = fread(buffer, 1, config_file_size, file);
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
    static void *eos_generated_library_handle = load_library_at_path(get_path_relative_to_current_module("EOSGenerated.dll"));
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

        iter = iter->next;
    }
	
	return eos_config;
}


//-------------------------------------------------------------------------
static std::filesystem::path get_path_for_eos_service_config()
{
	return get_path_relative_to_current_module(std::filesystem::path("../..") / "StreamingAssets" / "EOS" / "EpicOnlineServicesConfig.json");
}

//-------------------------------------------------------------------------
json_value_s* read_eos_config_as_json_value_from_file()
{
	std::filesystem::path path_to_config_json = get_path_for_eos_service_config();

	return read_config_json_as_json_from_path(path_to_config_json);
}

//-------------------------------------------------------------------------
void eos_create(EOSConfig& eosConfig)
{
	EOS_Platform_Options platform_options = {0};
	platform_options.ApiVersion = EOS_PLATFORM_OPTIONS_API_LATEST;
	platform_options.bIsServer = EOS_FALSE;
	platform_options.Flags = 0;
	platform_options.CacheDirectory = GetCacheDirectory();

	platform_options.EncryptionKey = eosConfig.encryptionKey.length() > 0 ? eosConfig.encryptionKey.c_str() : nullptr;
	platform_options.OverrideCountryCode = eosConfig.overrideCountryCode.length() > 0 ? eosConfig.overrideCountryCode.c_str() : nullptr;
	platform_options.OverrideLocaleCode = eosConfig.overrideLocaleCode.length() > 0 ? eosConfig.overrideLocaleCode.c_str() : nullptr;
	platform_options.ProductId = eosConfig.productID.c_str();
	platform_options.SandboxId = eosConfig.sandboxID.c_str();
	platform_options.DeploymentId = eosConfig.deploymentID.c_str();
	platform_options.ClientCredentials.ClientId = eosConfig.clientID.c_str();
	platform_options.ClientCredentials.ClientSecret = eosConfig.clientSecret.c_str();

    log_warn("run EOS_Platform_Create");
    eos_platform_handle = EOS_Platform_Create_ptr(&platform_options);
	if (!eos_platform_handle)
	{
		log_warn("failed to create the platform");
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
	log_warn("Trying to get a DLL path on a platform without DLL paths searching");
	return false;
#endif
}


//-------------------------------------------------------------------------
// Called by unity on load. It kicks off the work to load the DLL for Overlay
#if PLATFORM_32BITS
#pragma comment(linker, "/export:UnityPluginLoad=_UnityPluginLoad@4")
#endif
DLL_EXPORT(void) UnityPluginLoad(void*)
{
	fs::path DllPath;
	log_warn("On UnityPluginLoad");
	if (get_overlay_dll_path(&DllPath))
	{
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
			EOS_Initialize_ptr = load_function_with_name<EOS_Initialize_t>(s_eos_sdk_lib_handle, pick_if_32bit_else("_EOS_Initialize@4", "EOS_Initialize"));
			EOS_Shutdown_ptr = load_function_with_name<EOS_Shutdown_t>(s_eos_sdk_lib_handle, pick_if_32bit_else("_EOS_Shutdown@0", "EOS_Shutdown"));
			log_warn("fetch eos_platform_create");
			EOS_Platform_Create_ptr = load_function_with_name<EOS_Platform_Create_t>(s_eos_sdk_lib_handle, pick_if_32bit_else("_EOS_Platform_Create@4", "EOS_Platform_Create"));
			EOS_Platform_Release_ptr = load_function_with_name<EOS_Platform_Release_t>(s_eos_sdk_lib_handle, pick_if_32bit_else("_EOS_Platform_Release@4", "EOS_Platform_Release"));
			if (EOS_Initialize_ptr)
            {
                log_warn("start eos init");

                auto path_to_config_json = get_path_for_eos_service_config();
                json_value_s* eos_config_as_json = nullptr;

				eos_config_as_json = read_config_json_from_dll();

                if(!eos_config_as_json && std::filesystem::exists(path_to_config_json))
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
			else {
				log_warn("unable to find EOS_Initialize");
			}
		}
		else 
		{
			log_warn("Couldn't find dll "  SDK_DLL_NAME);
		}
	}
    else
    {
		log_warn("Unable to load EOS Overlay DLL");
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
}

//-------------------------------------------------------------------------
DLL_EXPORT(void) UnloadEOS()
{
	if (EOS_Shutdown_ptr)
	{
		log_warn("EOS shutdown");
		EOS_Shutdown_ptr();
	}
	if (s_eos_sdk_lib_handle)
	{
		log_warn("Unload eos sdk handle");
		unload_library(s_eos_sdk_lib_handle);
	}
}

DLL_EXPORT(void *) EOS_GetPlatformInterface()
{
	return eos_platform_handle;
}
