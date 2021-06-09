#include "pch.h"

#include "DLLHContext.h"
#include "DynamicLibraryLoaderHelper_Win32.h"

#if PLATFORM_WINDOWS

//-------------------------------------------------------------------------
void * DLLH_Win32_load_library_at_path(DLLHContext *ctx, const char *library_path)
{
	HMODULE handle = LoadLibraryA(library_path);
	return (void*)handle;
}

//-------------------------------------------------------------------------
void * DLLH_Win32_load_function_with_name(DLLHContext *ctx, void *library_handle, const char *function)
{
	HMODULE handle = (HMODULE)library_handle;
	return (void*)GetProcAddress(handle, function);
}

#endif
