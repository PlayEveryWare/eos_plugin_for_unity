#pragma once
#include <stddef.h>
#include <inttypes.h>

// These function need to be implemented on each platform
namespace platform
{
    void * alloc_aligned(size_t size_in_bytes, size_t alignment_in_bytes);
    void * realloc_aligned(void *pointer, size_t size_in_bytes, size_t alignment_in_bytes);

    size_t mem_usable_size(void* pointer);

    void free_aligned(void *pointer);
}

struct MemCounters
{
    int64_t currentMemoryAllocatedInBytes;
};
