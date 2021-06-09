#include <dlsym.h>

struct DLLHContext; 

//-------------------------------------------------------------------------
void * DLLH_Android_load_library_at_path(DLLHContext *ctx, const char *library_path)
{
	void *handle = dlopen(library_path, RTLD_LAZY);
	return handle;
}

//-------------------------------------------------------------------------
void * DLLH_Android_load_function_with_name(DLLHContext *ctx, void *library_handle, const char *function)
{
	return (void*)dlsym(library_handle, function);
}

