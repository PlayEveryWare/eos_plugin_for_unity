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

using System.Runtime.InteropServices;
using System.Reflection;
using System;

using size_t = System.UIntPtr;

public partial class SystemMemory
{
#if !(UNITY_ANDROID || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA_10_0) || UNITY_SWITCH
    public delegate IntPtr EOS_GenericAlignAlloc(size_t sizeInBytes, size_t alignmentInBytes);
    public delegate IntPtr EOS_GenericAlignRealloc(IntPtr ptr, size_t sizeInBytes, size_t alignmentInBytes);
    public delegate void EOS_GenericFree(IntPtr ptr);

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

    [DllImport("__Internal")]
    static public extern IntPtr Mem_generic_align_alloc(size_t size_in_bytes, size_t alignment_in_bytes);

    [DllImport("__Internal")]
    static public extern IntPtr Mem_generic_align_realloc(IntPtr ptr, size_t size_in_bytes, size_t alignment_in_bytes);

    [DllImport("__Internal")]
    static public extern void Mem_generic_free(IntPtr ptr);
#endif
}
