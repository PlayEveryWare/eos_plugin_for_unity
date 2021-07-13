#include "pch.h"
#include "Memory.h"

STATIC_EXPORT(void *) Mem_generic_align_alloc(size_t size_in_bytes, size_t alignment_in_bytes)
{
    void * to_return = nullptr;

    to_return = platform::alloc_aligned(size_in_bytes, alignment_in_bytes);

    return to_return;
}

STATIC_EXPORT(void *) Mem_generic_align_realloc(void *ptr, size_t size_in_bytes, size_t alignment_in_bytes)
{
    void * to_return = nullptr;

    to_return = platform::realloc_aligned(ptr, size_in_bytes, alignment_in_bytes);

    return to_return;
}

STATIC_EXPORT(void) Mem_generic_free(void *ptr)
{
    platform::free_aligned(ptr);
}
