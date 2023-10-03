// DynamicLibraryLoaderHelper.cpp : Defines the functions for the static library.
//

#include <assert.h>
#include <dlfcn.h>
#include <stdio.h>
#include <map>
#include <string>
#include <libgen.h>
#include <link.h>

#define STATIC_EXPORT(return_type) extern "C" return_type

std::map <std::string,std::string> baseNameToPath;

struct DLLHContext
{
};

//-------------------------------------------------------------------------
STATIC_EXPORT(void*) LoadLibrary(const char *library_path)
{
    std::string filename = std::string(basename((char*)library_path));
    std::string stemname = filename.substr(0, filename.find_last_of("."));
   
    void* handle = dlopen(library_path, RTLD_NOW);
    if(handle == nullptr)
    {
        return nullptr;
    }
    
    baseNameToPath[stemname] = std::string(library_path);

    return handle;
}

//-------------------------------------------------------------------------
// pretend windows like function
STATIC_EXPORT(bool) FreeLibrary(void *library_handle)
{
    dlclose(library_handle);
    
    if(dlerror())
    {
        return false;
    }
    
    return true;
}

//-------------------------------------------------------------------------
STATIC_EXPORT(void*) GetModuleHandle(const char *stemname)
{
    if(baseNameToPath.find(stemname) == baseNameToPath.end())
    {
        return nullptr;
    }
        
    void *to_return = dlopen(baseNameToPath[stemname].c_str(), RTLD_NOLOAD);
    
    if(to_return == nullptr)
    {
        baseNameToPath.erase(stemname);
    }
    // dlopen increments the ref handle, so make sure to 
    // release the ref handle. See `man dlopen`
    if (to_return)
    {
        dlclose(to_return);
    }

    return to_return;
}

//-------------------------------------------------------------------------
STATIC_EXPORT(void*) GetProcAddress(void *library_handle, const char *function_name)
{
    return dlsym(library_handle, function_name);
}

//-------------------------------------------------------------------------
STATIC_EXPORT(void) GetError()
{
    char *errstr;

    errstr = dlerror();
    
    freopen("debug.txt", "w", stdout);
    printf ("TryDynamicLinking \n");
    if (errstr != NULL)
    printf ("A dynamic linking error occurred: (%s)\n", errstr);
    fclose (stdout);

}

static int print_lib_callback(struct dl_phdr_info *info, size_t size, void *data)
{
    int* counter = (int*)data;
    printf("lib num %d : %s\n",*counter,info->dlpi_name);
    (*counter)++;
    return 0;
}

STATIC_EXPORT(void) PrintLibs()
{
    freopen("libs.txt", "w", stdout);
    int counter = 0;
    dl_iterate_phdr(print_lib_callback, &counter);
    fclose (stdout);
}

//-------------------------------------------------------------------------
void * DLLH_linux_load_library_at_path(DLLHContext *ctx, const char *library_path)
{
    void *to_return = dlopen(library_path, RTLD_NOW);
   
    return to_return; 
}

//-------------------------------------------------------------------------
// TODO: Handle the actual module instead of all symbols
void * DLLH_linux_load_function_with_name(DLLHContext *ctx, void *library_handle, const char *function)
{
    void *output_ptr = nullptr;

    output_ptr = dlsym(library_handle, function);

    return output_ptr;
}

//-------------------------------------------------------------------------
// Create heap data for storing random things, if need be on a given platform
STATIC_EXPORT(void *) DLLH_create_context()
{
    return new DLLHContext();
}

//-------------------------------------------------------------------------
STATIC_EXPORT(void) DLLH_destroy_context(void *context)
{
    delete static_cast<DLLHContext *>(context);
}

//-------------------------------------------------------------------------
STATIC_EXPORT(void *) DLLH_load_library_at_path(void *ctx, const char *library_path)
{
    if (ctx == nullptr) {
        return nullptr;
    }

    DLLHContext *dllh_ctx = static_cast<DLLHContext*>(ctx);
    void *to_return = nullptr;
    
    to_return = DLLH_linux_load_library_at_path(dllh_ctx, library_path);

    return to_return;
}

//-------------------------------------------------------------------------
// This returns a bare function pointer that is only valid as long as the library_handle and context are
// valid
STATIC_EXPORT(void *) DLLH_load_function_with_name(void *ctx, void *library_handle, const char *function)
{
    void *to_return = nullptr;
    DLLHContext *dllh_ctx = static_cast<DLLHContext*>(ctx);

    to_return = DLLH_linux_load_function_with_name(dllh_ctx, library_handle, function);

    return to_return;
}

//-------------------------------------------------------------------------
// TODO: unload the library correct? I don't know if that's actually a good
// idea on linux or not
STATIC_EXPORT(void) DLLH_unload_library_at_path(const char *library_path)
{
}

