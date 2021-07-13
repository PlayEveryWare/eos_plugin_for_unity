#include "pch.h"
#include "Memory_WinCRT.h"

#include <stdlib.h>

STATIC_EXPORT(void *) Mem_generic_align_alloc(size_t size_in_bytes, size_t alignment_in_bytes)
{
    void * to_return = nullptr;

#if _WIN64 || _WIN32
    to_return = Mem_win_crt_align_alloc(size_in_bytes, alignment_in_bytes);
#else
    to_return = aligned_alloc(size_in_bytes, alignment_in_bytes);
#endif

    return to_return;
}

STATIC_EXPORT(void *) Mem_generic_align_realloc(void *ptr, size_t size_in_bytes, size_t alignment_in_bytes)
{
    void * to_return = nullptr;

#if _WIN64 || _WIN32
    to_return = Mem_win_crt_align_realloc(ptr, size_in_bytes);
#else
#error "Missing implementation for Mem_generic_align_realloc"
#endif

    return to_return;
}

STATIC_EXPORT(void) Mem_generic_free(void *ptr)
{
#if _WIN64 || _WIN32
    Mem_win_crt_generic_free_wrapper(ptr);
#else
#error "Missing implementation for Mem_generic_free"
#endif
}
