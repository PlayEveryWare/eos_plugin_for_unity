// DynamicLibraryLoaderHelper.cpp : Defines the functions for the static library.
//

#include "pch.h"
#include "framework.h"
#include "DLLHContext.h"
#include <assert.h>


// Create heap data for storing random things, if need be on a given platform
STATIC_EXPORT(void *) DLLH_create_context()
{
    return new DLLHContext();
}

STATIC_EXPORT(void) DLLH_destroy_context(void *context)
{
    delete static_cast<DLLHContext *>(context);
}

STATIC_EXPORT(void *) DLLH_load_library_at_path(void *ctx, const char *library_path)
{
    if (ctx == nullptr) {
        return nullptr;
    }

    DLLHContext *dllh_ctx = static_cast<DLLHContext*>(ctx);
    void *to_return = nullptr;
    
    to_return = platform::DLLH_load_library_at_path(dllh_ctx, library_path);

    return to_return;
}

// This returns a bare function pointer that is only valid as long as the library_handle and context are
// valid
STATIC_EXPORT(void *) DLLH_load_function_with_name(void *ctx, void *library_handle, const char *function)
{
    void *to_return = nullptr;
    DLLHContext *dllh_ctx = static_cast<DLLHContext*>(ctx);

    to_return = platform::DLLH_load_function_with_name(dllh_ctx, library_handle, function);

    return to_return;
}
