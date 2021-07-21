#pragma once

struct DLLHContext;

void * DLLH_Win32_load_library_at_path(DLLHContext *ctx, const char *library_path);

bool DLLH_Win32_unload_library_at_path(void *ctx, void *library_handle);

void * DLLH_Win32_load_function_with_name(DLLHContext *ctx, void *library_handle, const char *function);
