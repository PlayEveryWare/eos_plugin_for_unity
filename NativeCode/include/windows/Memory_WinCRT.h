#pragma once
#include <stddef.h>

void * Mem_win_crt_align_alloc(size_t size_in_bytes, size_t alignment_in_bytes);
void * Mem_win_crt_align_realloc(void *ptr, size_t size_in_bytes, size_t alignment_in_bytes);
void Mem_win_crt_generic_free_wrapper(void *ptr);