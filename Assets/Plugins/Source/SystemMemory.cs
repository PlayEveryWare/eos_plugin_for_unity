using System.Runtime.InteropServices;
using System.Reflection;
using System;

using size_t = System.UIntPtr;

public partial class SystemMemory
{
#if !UNITY_ANDROID
    static public IntPtr GenericAlignAlloc(size_t sizeInBytes, size_t alignmentInBytes)
    {
        return Mem_generic_align_alloc(sizeInBytes, alignmentInBytes);
    }

    static public IntPtr GenericAlignRealloc(IntPtr ptr, size_t sizeInBytes, size_t alignmentInBytes)
    {
        return Mem_generic_align_realloc(ptr, sizeInBytes, alignmentInBytes);
    }

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
