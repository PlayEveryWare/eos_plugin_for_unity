#include "pch.h"
#include "Memory_WinCRT.h"

#if _WIN64 || _WIN32
#define WINDOWS_CRT_PLATFORM 1
#endif

#if WINDOWS_CRT_PLATFORM
#include <malloc.h>
#include <stdlib.h>
#endif

#if WINDOWS_CRT_PLATFORM
void * Mem_win_crt_align_alloc(size_t size_in_bytes, size_t alignment_in_bytes)
{
	return _aligned_malloc(size_in_bytes, alignment_in_bytes);
}

void * Mem_win_crt_align_realloc(void *ptr, size_t size_in_bytes, size_t alignment_in_bytes)
{
	return _aligned_realloc(ptr, size_in_bytes, alignment_in_bytes);
}

void Mem_win_crt_generic_free_wrapper(void *ptr)
{
    _aligned_free(ptr);
}

#endif
