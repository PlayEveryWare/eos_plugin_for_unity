// DynamicLibraryLoaderHelper.cpp : Defines the functions for the static library.
//

#include "pch.h"
#include "framework.h"
#include "DLLHContext.h"
#include <assert.h>

#if PLATFORM_WINDOWS
#include "DynamicLibraryLoaderHelper_Win32.h"
#endif


#define DLL_EXPORT(return_value) extern "C" __declspec(dllexport) return_value  __stdcall

//#define FUN_EXPORT(return_value) STATIC_EXPORT(return_value)
#define FUN_EXPORT(return_value) DLL_EXPORT(return_value)

// Create heap data for storing random things, if need be on a given platform
FUN_EXPORT(void *) DLLH_create_context()
{
    return new DLLHContext();
}

//-------------------------------------------------------------------------
FUN_EXPORT(void) DLLH_destroy_context(void *context)
{
    delete static_cast<DLLHContext *>(context);
}

//-------------------------------------------------------------------------
FUN_EXPORT(void *) DLLH_load_library_at_path(void *ctx, const char *library_path)
{
    if (ctx == nullptr) {
        return nullptr;
    }

    DLLHContext *dllh_ctx = static_cast<DLLHContext*>(ctx);
    void *to_return = nullptr;
    
    to_return = platform::DLLH_load_library_at_path(dllh_ctx, library_path);

    return to_return;
}


//-------------------------------------------------------------------------
FUN_EXPORT(bool) DLLH_unload_library_at_path(void *ctx, void *library_handle)
{
#if PLATFORM_WINDOWS
    return DLLH_Win32_unload_library_at_path(ctx, library_handle);
#endif
}

//-------------------------------------------------------------------------
// This returns a bare function pointer that is only valid as long as the library_handle and context are
// valid
FUN_EXPORT(void *) DLLH_load_function_with_name(void *ctx, void *library_handle, const char *function)
{
    void *to_return = nullptr;
    DLLHContext *dllh_ctx = static_cast<DLLHContext*>(ctx);

    to_return = platform::DLLH_load_function_with_name(dllh_ctx, library_handle, function);

    return to_return;
}
