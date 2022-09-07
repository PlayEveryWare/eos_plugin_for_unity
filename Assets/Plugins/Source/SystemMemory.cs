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

using System.Runtime.InteropServices;
using System.Reflection;
using System;
using PlayEveryWare.EpicOnlineServices;

using size_t = System.UIntPtr;

public partial class SystemMemory
{
#if !(UNITY_ANDROID || UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_WSA_10_0) || UNITY_SWITCH || UNITY_GAMECORE || UNITY_PS5 || UNITY_PS4

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct MemCounters 
    {
        public Int64 currentMemoryAllocatedInBytes;
    };

    public delegate IntPtr EOS_GenericAlignAlloc(size_t sizeInBytes, size_t alignmentInBytes);
    public delegate IntPtr EOS_GenericAlignRealloc(IntPtr ptr, size_t sizeInBytes, size_t alignmentInBytes);
    public delegate void EOS_GenericFree(IntPtr ptr);

    static private EOS_GenericAlignAlloc GenericAlignAllocDelegate;
    static private EOS_GenericAlignRealloc GenericAlignReallocDelegate;
    static private EOS_GenericFree GenericFreeDelegate;

    static public readonly IntPtr GenericAlignAllocFunctionPointer;
    static public readonly IntPtr GenericAlignReallocFunctionPointer;
    static public readonly IntPtr GenericFreeFunctionPointer;

    static SystemMemory()
    {
        GenericAlignAllocDelegate = new EOS_GenericAlignAlloc(GenericAlignAlloc);
        GenericAlignReallocDelegate = new EOS_GenericAlignRealloc(GenericAlignRealloc);
        GenericFreeDelegate = new EOS_GenericFree(GenericFree);

        GenericAlignAllocFunctionPointer = Marshal.GetFunctionPointerForDelegate(GenericAlignAllocDelegate);
        GenericAlignReallocFunctionPointer = Marshal.GetFunctionPointerForDelegate(GenericAlignReallocDelegate);
        GenericFreeFunctionPointer = Marshal.GetFunctionPointerForDelegate(GenericFreeDelegate);
    }

    [AOT.MonoPInvokeCallback(typeof(EOS_GenericAlignAlloc))]
    static public IntPtr GenericAlignAlloc(size_t sizeInBytes, size_t alignmentInBytes)
    {
        return Mem_generic_align_alloc(sizeInBytes, alignmentInBytes);
    }

    //-------------------------------------------------------------------------
    [AOT.MonoPInvokeCallback(typeof(EOS_GenericAlignRealloc))]
    static public IntPtr GenericAlignRealloc(IntPtr ptr, size_t sizeInBytes, size_t alignmentInBytes)
    {
        return Mem_generic_align_realloc(ptr, sizeInBytes, alignmentInBytes);
    }

    //-------------------------------------------------------------------------
    [AOT.MonoPInvokeCallback(typeof(EOS_GenericFree))]
    static public void GenericFree(IntPtr ptr)
    {
        Mem_generic_free(ptr);
    }

    //-------------------------------------------------------------------------
    static public void GetAllocatorFunctions(out IntPtr alloc, out IntPtr realloc, out IntPtr free)
    {
#if ENABLE_GET_ALLOCATOR_FUNCTION
        Mem_GetAllocatorFunctions(out alloc, out realloc, out free);
#else
        alloc = GenericAlignAllocFunctionPointer;
        realloc = GenericAlignReallocFunctionPointer;
        free = GenericFreeFunctionPointer;
#endif
    }

    private const string DLLHBinaryName =
#if UNITY_GAMECORE
        "DynamicLibraryLoaderHelper";
#else
        "__Internal";
#endif

    [DllImport(DLLHBinaryName)]
    static public extern IntPtr Mem_generic_align_alloc(size_t size_in_bytes, size_t alignment_in_bytes);

    [DllImport(DLLHBinaryName)]
    static public extern IntPtr Mem_generic_align_realloc(IntPtr ptr, size_t size_in_bytes, size_t alignment_in_bytes);

    [DllImport(DLLHBinaryName)]
    static public extern void Mem_generic_free(IntPtr ptr);

#if !UNITY_IOS
    [DllImport(DLLHBinaryName)]
    static public extern void Mem_GetAllocationCounters(out MemCounters data);
#endif

#if ENABLE_GET_ALLOCATOR_FUNCTION
    [DllImport(DLLHBinaryName)]
    private static extern void Mem_GetAllocatorFunctions(out IntPtr alloc, out IntPtr realloc, out IntPtr free);
#endif

#endif
}
