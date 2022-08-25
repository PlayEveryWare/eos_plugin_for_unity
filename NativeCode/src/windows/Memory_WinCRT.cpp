#include "pch.h"
#include "Memory.h"

#if _WIN64 || _WIN32
#define WINDOWS_CRT_PLATFORM 1
#endif

#if WINDOWS_CRT_PLATFORM
#include <malloc.h>
#include <stdlib.h>
#endif

#if WINDOWS_CRT_PLATFORM
void * platform::alloc_aligned(size_t size_in_bytes, size_t alignment_in_bytes)
{
    return _aligned_malloc(size_in_bytes, alignment_in_bytes);
}

void * platform::realloc_aligned(void *pointer, size_t size_in_bytes, size_t alignment_in_bytes)
{
    return _aligned_realloc(pointer, size_in_bytes, alignment_in_bytes);
}

void platform::free_aligned(void *ptr)
{
    _aligned_free(ptr);
}

size_t platform::mem_usable_size(void* pointer)
{
    return _msize(pointer);
}


#endif
