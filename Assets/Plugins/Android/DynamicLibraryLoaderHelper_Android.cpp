#include <dlfcn.h>

#define STATIC_EXPORT(return_type) extern "C" return_type
#define DLL_EXPORT(return_value) extern "C" __declspec(dllexport) return_value  __stdcall

//#define FUN_EXPORT(return_value) DLL_EXPORT(return_value)

#define FUN_EXPORT(return_value) return_value

struct DLLHContext
{
}; 

//-------------------------------------------------------------------------
void * DLLH_Android_load_library_at_path(DLLHContext *ctx, const char *library_path)
{
	void *handle = dlopen(library_path, RTLD_LAZY);
	return handle;
}

//-------------------------------------------------------------------------
bool DLLH_Android_unload_library_at_path(DLLHContext *ctx, void *library_handle)
{
    return dlclose(library_handle) == 0;
}

//-------------------------------------------------------------------------
void * DLLH_Android_load_function_with_name(DLLHContext *ctx, void *library_handle, const char *function)
{
	return (void*)dlsym(library_handle, function);
}

//-------------------------------------------------------------------------
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
    DLLHContext *dllh_ctx = static_cast<DLLHContext*>(ctx);
    void *to_return = nullptr;

    to_return = DLLH_Android_load_library_at_path(dllh_ctx, library_path);

    return to_return;
}

//-------------------------------------------------------------------------
FUN_EXPORT(bool) DLLH_unload_library_at_path(void *ctx, void *library_handle)
{
    DLLHContext* dllh_ctx = static_cast<DLLHContext*>(ctx);
    return DLLH_Android_unload_library_at_path(dllh_ctx, library_handle);
}

//-------------------------------------------------------------------------
// This returns a bare function pointer that is only valid as long as the library_handle and context are
// valid
FUN_EXPORT(void *) DLLH_load_function_with_name(void *ctx, void *library_handle, const char *function)
{
    void *to_return = nullptr;
    DLLHContext *dllh_ctx = static_cast<DLLHContext*>(ctx);

    to_return = DLLH_Android_load_function_with_name(dllh_ctx, library_handle, function);

    return to_return;
}
