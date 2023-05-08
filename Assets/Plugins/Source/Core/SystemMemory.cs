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
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

#if UNITY_PS5 || UNITY_PS4 || UNITY_GAMECORE_XBOXONE || UNITY_GAMECORE_SCARLETT
#define ENABLE_GET_ALLOCATOR_FUNCTION
#endif

#if UNITY_IOS && UNITY_STANDALONE
#define DLLHELPER_HAS_INTERNAL_LINKAGE
#endif

#if UNITY_STANDALONE || UNITY_EDITOR_WIN
#define DYNAMIC_MEMORY_ALLOCATION_AVAILABLE
#endif

using System.Runtime.InteropServices;
using System.Reflection;
using System;
using PlayEveryWare.EpicOnlineServices;

using size_t = System.UIntPtr;

//-------------------------------------------------------------------------
// Generic interface for allocating native memory that conforms to the EOS SDK
public partial class SystemMemory
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct MemCounters 
    {
        public Int64 currentMemoryAllocatedInBytes;
    };

    public delegate IntPtr EOS_GenericAlignAlloc(size_t sizeInBytes, size_t alignmentInBytes);
    public delegate IntPtr EOS_GenericAlignRealloc(IntPtr ptr, size_t sizeInBytes, size_t alignmentInBytes);
    public delegate void EOS_GenericFree(IntPtr ptr);


    [AOT.MonoPInvokeCallback(typeof(EOS_GenericAlignAlloc))]
    static public IntPtr GenericAlignAlloc(size_t sizeInBytes, size_t alignmentInBytes)
    {
#if DYNAMIC_MEMORY_ALLOCATION_AVAILABLE
        return Mem_generic_align_alloc(sizeInBytes, alignmentInBytes);
#else
        return IntPtr.Zero;
#endif
    }

    //-------------------------------------------------------------------------
    [AOT.MonoPInvokeCallback(typeof(EOS_GenericAlignRealloc))]
    static public IntPtr GenericAlignRealloc(IntPtr ptr, size_t sizeInBytes, size_t alignmentInBytes)
    {
#if DYNAMIC_MEMORY_ALLOCATION_AVAILABLE
        return Mem_generic_align_realloc(ptr, sizeInBytes, alignmentInBytes);
#else
        return IntPtr.Zero;
#endif
    }

    //-------------------------------------------------------------------------
    [AOT.MonoPInvokeCallback(typeof(EOS_GenericFree))]
    static public void GenericFree(IntPtr ptr)
    {
#if DYNAMIC_MEMORY_ALLOCATION_AVAILABLE
        Mem_generic_free(ptr);
#endif
    }

    //-------------------------------------------------------------------------
    static public void GetAllocatorFunctions(out IntPtr alloc, out IntPtr realloc, out IntPtr free)
    {
#if ENABLE_GET_ALLOCATOR_FUNCTION
        Mem_GetAllocatorFunctions(out alloc, out realloc, out free);
#else
        alloc = IntPtr.Zero;
        realloc = IntPtr.Zero;
        free = IntPtr.Zero;
#endif
    }

    private const string DLLHBinaryName =
#if DLLHELPER_HAS_INTERNAL_LINKAGE
        "__Internal";
#else
        "DynamicLibraryLoaderHelper";

#endif

#if DYNAMIC_MEMORY_ALLOCATION_AVAILABLE
    [DllImport(DLLHBinaryName)]
    static public extern IntPtr Mem_generic_align_alloc(size_t size_in_bytes, size_t alignment_in_bytes);

    [DllImport(DLLHBinaryName)]
    static public extern IntPtr Mem_generic_align_realloc(IntPtr ptr, size_t size_in_bytes, size_t alignment_in_bytes);

    [DllImport(DLLHBinaryName)]
    static public extern void Mem_generic_free(IntPtr ptr);

    // This is currently not implemented
#if ENABLE_GET_ALLOCATION_COUNTERS
    [DllImport(DLLHBinaryName)]
    static public extern void Mem_GetAllocationCounters(out MemCounters data);
#endif

#if ENABLE_GET_ALLOCATOR_FUNCTION
    [DllImport(DLLHBinaryName)]
    private static extern void Mem_GetAllocatorFunctions(out IntPtr alloc, out IntPtr realloc, out IntPtr free);
#endif
#endif

}
