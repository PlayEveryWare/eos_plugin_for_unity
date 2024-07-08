/*
 * Copyright (c) 2021 PlayEveryWare
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

#include <stdlib.h>

#define STATIC_EXPORT(return_type) extern "C" return_type

//-------------------------------------------------------------------------
// Wrapper around the standard aligned_alloc that asserts on error cases.
// Useful for debugging memory issues.
STATIC_EXPORT(void *) Mem_generic_align_alloc(size_t size_in_bytes, size_t alignment_in_bytes)
{
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
