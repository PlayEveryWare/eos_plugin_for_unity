#pragma once

#include "DLLHContextPlatform.h"

struct DLLHContext;

namespace platform
{
    // Needs to be defined in DLLHContextPlatform.h
    struct PlatformSpecificContext;

    void * DLLH_load_library_at_path(DLLHContext* ctx, const char* library_path);
    void * DLLH_load_function_with_name(DLLHContext* ctx, void* library_handle, const char* function);
    bool DLLH_unload_library_at_path(DLLHContext* ctx, void *library_handle);
}

struct DLLHContext
{
    platform::PlatformSpecificContext platform_specific_ctx;
};
