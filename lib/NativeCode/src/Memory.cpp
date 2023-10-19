#include "pch.h"
#include "Memory.h"
#include <atomic>
#include <mutex>
#include <vector>

//#define DLLH_ENABLE_MEMORY_COUNTER 1
//#define DLLH_USE_ATOMIC_MEMORY_COUNTER

#ifdef DLLH_ENABLE_MEMORY_COUNTER

#if DLLH_USE_ATOMIC_MEMORY_COUNTER
static std::atomic_int64_t s_currentMemoryAllocatedInBytes(0);
#else
int64_t s_currentMemoryAllocatedInBytes;
#endif

std::mutex g_allocated_bytes_mutex;
#endif
std::vector<std::tuple<void*, size_t>> g_allocated_bytes;

//-------------------------------------------------------------------------
int64_t readCurrentMemoryAllocatedInBytes()
{
#if DLLH_USE_ATOMIC_MEMORY_COUNTER
    return s_currentMemoryAllocatedInBytes.load();
#else
#if DLLH_ENABLE_MEMORY_COUNTER
    return s_currentMemoryAllocatedInBytes;
#else
    return 0;
#endif
#endif
}

//-------------------------------------------------------------------------
static int64_t get_allocated_bytes_total()
{
    int64_t total = 0;
    //std::lock_guard<std::mutex> scope_lock(g_allocated_bytes_mutex);
    for (auto iter = g_allocated_bytes.begin(); iter != g_allocated_bytes.end(); ++iter)
    {
        total += std::get<1>(*iter);
    }
    return total;
}

//-------------------------------------------------------------------------
static std::vector<std::tuple<void*, size_t>>::iterator get_matching_ptr(void* ptr)
{
#ifdef DLLH_ENABLE_MEMORY_COUNTER
    if (ptr != nullptr)
    {
        //std::lock_guard<std::mutex> scope_lock(g_allocated_bytes_mutex);
        for(auto iter = g_allocated_bytes.begin(); iter != g_allocated_bytes.end(); ++iter)
        {
            if (std::get<0>(*iter) == ptr)
            {
                return iter;
            }
        }
    }
#endif
    return g_allocated_bytes.end();
}

//-------------------------------------------------------------------------
static size_t get_alloc_size(void* ptr)
{
#ifdef DLLH_ENABLE_MEMORY_COUNTER
    auto iter = get_matching_ptr(ptr);

    if (iter != g_allocated_bytes.end())
    {
        return std::get<1>(*iter);
    }
#else
    std::ignore = ptr;
#endif

    return 0;
}

//-------------------------------------------------------------------------
static void remove_pointer(void* ptr)
{
#ifdef DLLH_ENABLE_MEMORY_COUNTER
    auto iter = get_matching_ptr(ptr);
    if (iter != g_allocated_bytes.end())
    {
        // This crashes sometimes, as the ptr that's passed in isn't always valid, maybe????
        size_t previous_size = get_alloc_size(ptr);

        //NN_LOG("MEM: remove ptr: (%p)", ptr);
        s_currentMemoryAllocatedInBytes -= previous_size;
        //std::lock_guard<std::mutex> scope_lock(g_allocated_bytes_mutex);
        auto end_value = g_allocated_bytes.back();
        *iter = end_value;
        g_allocated_bytes.pop_back();
    }
#else
    std::ignore = ptr;
#endif
}

//-------------------------------------------------------------------------
static void add_pointer(void* ptr, size_t size_in_bytes)
{
#ifdef DLLH_ENABLE_MEMORY_COUNTER
    //std::lock_guard<std::mutex> scope_lock(g_allocated_bytes_mutex);
    g_allocated_bytes.push_back(std::tuple<void*,size_t>(ptr, size_in_bytes));
    s_currentMemoryAllocatedInBytes += size_in_bytes;
#else
    std::ignore = ptr, size_in_bytes;
#endif
}
//-------------------------------------------------------------------------
//-------------------------------------------------------------------------

//-------------------------------------------------------------------------
FUN_EXPORT(void *) Mem_generic_align_alloc(size_t size_in_bytes, size_t alignment_in_bytes)
{
    void * to_return = nullptr;

    to_return = platform::alloc_aligned(size_in_bytes, alignment_in_bytes);
    add_pointer(to_return, size_in_bytes);

    return to_return;
}

//-------------------------------------------------------------------------
FUN_EXPORT(void *) Mem_generic_align_realloc(void *ptr, size_t size_in_bytes, size_t alignment_in_bytes)
{
    void * to_return = nullptr;

    to_return = platform::realloc_aligned(ptr, size_in_bytes, alignment_in_bytes);

    remove_pointer(ptr);
    add_pointer(to_return, size_in_bytes);

    return to_return;
}

//-------------------------------------------------------------------------
FUN_EXPORT(void) Mem_generic_free(void *ptr)
{
    remove_pointer(ptr);

    platform::free_aligned(ptr);
}

//-------------------------------------------------------------------------
FUN_EXPORT(void) Mem_GetAllocatorFunctions(void** alloc, void** realloc, void** free)
{
    *alloc = reinterpret_cast<void*>(&Mem_generic_align_alloc);
    *realloc = reinterpret_cast<void*>(&Mem_generic_align_realloc);
    *free = reinterpret_cast<void*>(&Mem_generic_free);
}

//-------------------------------------------------------------------------
FUN_EXPORT(void) Mem_GetAllocationCounters(void* data)
{
#ifdef DLLH_ENABLE_MEMORY_COUNTER
    MemCounters* mem_counters = reinterpret_cast<MemCounters*>(data);

    mem_counters->currentMemoryAllocatedInBytes = readCurrentMemoryAllocatedInBytes();
#else
    std::ignore = data;
#endif
}
