#include "pch.h"

#include "DLLHContext.h"

#define _SILENCE_CXX17_CODECVT_HEADER_DEPRECATION_WARNING
#include <locale>
#include <codecvt>

#if PLATFORM_WINDOWS



static void show_log_as_dialog(std::wstring log_string)
{
    MessageBoxW(NULL, log_string.c_str(), L"Warning", MB_ICONWARNING);
}
//-------------------------------------------------------------------------
static void show_log_as_dialog(const char* log_string)
{
    MessageBoxA(NULL, log_string, "Warning", MB_ICONWARNING);

}

//-------------------------------------------------------------------------
//
static bool is_uwp()
{
    static int has_package_family_name = -1;
    typedef LONG (__stdcall *GetPackageFamilyName_t)(HANDLE, UINT32*, PWSTR);
    static GetPackageFamilyName_t GetPackageFamilyName_ptr = NULL;

    show_log_as_dialog("checking if is_uwp");
    // hasn't run before
    if (has_package_family_name == -1)
    {
        GetPackageFamilyName_ptr = (GetPackageFamilyName_t)GetProcAddress(GetModuleHandle(TEXT("kernel32")), "GetPackageFamilyName");
        if (GetPackageFamilyName_ptr != NULL)
        {
            show_log_as_dialog("looking for package name");
            UINT32 size = 0;
            auto result = GetPackageFamilyName_ptr(GetCurrentProcess(), &size, NULL);
            if(result == ERROR_INSUFFICIENT_BUFFER)
            {
                show_log_as_dialog("has package name");
                has_package_family_name = 1;
            }
            else
            {
                show_log_as_dialog("Doesn't have package name?");
                has_package_family_name = 0;
            }
        }
        else
        {
            show_log_as_dialog("Couldn't find GetPackageFamilyName");
            has_package_family_name = 0;
        }
    }

    return has_package_family_name == 1;
}

//-------------------------------------------------------------------------
void * platform::DLLH_load_library_at_path(DLLHContext *ctx, const char *library_path)
{
    HMODULE handle = NULL;
    if (is_uwp())
    {
        std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> converter;
        std::wstring library_path_as_wide_str = converter.from_bytes(library_path);
        show_log_as_dialog(library_path);
        show_log_as_dialog(library_path_as_wide_str);
        handle = LoadPackagedLibrary(library_path_as_wide_str.c_str(), 0);
    }
    else
    {
        handle = LoadLibraryA(library_path);
    }

    return (void*)handle;
}

//-------------------------------------------------------------------------
bool platform::DLLH_unload_library_at_path(DLLHContext*ctx, void *library_handle)
{
    HMODULE handle = (HMODULE)library_handle;
    return FreeLibrary(handle);
}

//-------------------------------------------------------------------------
void * platform::DLLH_load_function_with_name(DLLHContext *ctx, void *library_handle, const char *function)
{
    HMODULE handle = (HMODULE)library_handle;
    return (void*)GetProcAddress(handle, function);
}

//-------------------------------------------------------------------------
STATIC_EXPORT(void *) DLLH_Win32_get_module_handle(DLLHContext *ctx, const char *module_name)
{
    return GetModuleHandleA(module_name);
}

#endif
