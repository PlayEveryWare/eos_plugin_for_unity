#include "pch.h"

#include "DLLHContext.h"

#if PLATFORM_WINDOWS

//-------------------------------------------------------------------------
void * platform::DLLH_load_library_at_path(DLLHContext *ctx, const char *library_path)
{
    HMODULE handle = LoadLibraryA(library_path);
    return (void*)handle;
}

//-------------------------------------------------------------------------
void * platform::DLLH_load_function_with_name(DLLHContext *ctx, void *library_handle, const char *function)
{
    HMODULE handle = (HMODULE)library_handle;
    return (void*)GetProcAddress(handle, function);
}

#endif
