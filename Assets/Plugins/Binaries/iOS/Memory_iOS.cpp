#include <stdlib.h>

#define STATIC_EXPORT(return_type) extern "C" return_type

//-------------------------------------------------------------------------
// Wrapper around the standard aligned_alloc that asserts on error cases.
// Useful for debugging memory issues.
STATIC_EXPORT(void *) Mem_generic_align_alloc(size_t size_in_bytes, size_t alignment_in_bytes)
{
    //TODO: replace with posix version so we can support lower iOS versions
    void *to_return = aligned_alloc(alignment_in_bytes, size_in_bytes);

   return to_return;
}

//-------------------------------------------------------------------------
// Wrapper around the standard c function for reallocating memory.
// Has asserts to ensure that memory allocation is working.
// Handles case where size_in_bytes is 0 in the EOS, which isn't handled by the ios version of 
// realloc
STATIC_EXPORT(void *) Mem_generic_align_realloc(void *ptr, size_t size_in_bytes, size_t alignment_in_bytes)
{
       // Some objects in EOS try to realloc with a zero sized increase in bytes
       if (size_in_bytes == 0) {
               return ptr;
       }

       if (ptr == nullptr) {
               return Mem_generic_align_alloc(size_in_bytes, alignment_in_bytes);
       }

       void *to_return = realloc(ptr, size_in_bytes);
       return to_return;
}

//-------------------------------------------------------------------------
// Wrapper around free for EOS. One could just use free() and pass that in,
// but for completeness it's included here
STATIC_EXPORT(void) Mem_generic_free(void *ptr)
{
       free(ptr);
}

//-------------------------------------------------------------------------
// wrapper around malloc, Originally written for ios SDK, but could be used 
// elsewhere.
STATIC_EXPORT(void *) Mem_generic_allocator(size_t size)
{
       return malloc(size);
}

//-------------------------------------------------------------------------
// The matching wrapper function for generic_allocator
STATIC_EXPORT(void) Mem_generic_deallocate(void* to_deallocate, size_t size)
{
       free(to_deallocate);
}
