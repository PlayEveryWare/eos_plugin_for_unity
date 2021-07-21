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

/*
 * Example of using this 
#if USE_RELOABLE_DELEGATES
        private delegate IntPtr EOS_Platform_Create_Del(ref OptionsInternal options);
        private static EOS_Platform_Create_Del EOS_Platform_Create;
#else
        [DllImport(Config.BinaryName)]
		private static extern IntPtr EOS_Platform_Create(ref OptionsInternal options);
#endif
*/
public partial class SystemDynamicLibrary
{

    // These are maintained as DllImports instead of using the DLLH in the editor
    // so that the Editor won't hold a lock on the DLL, which can hurt iteration time
    // when testing new things on the DLL.
#if UNITY_EDITOR_WIN
    // In theory, its possible to use **Internal to get the
    // default system libraries.
    private const string Kernel32BinaryName = "kernel32";

    [DllImport(Kernel32BinaryName, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool FreeLibrary(IntPtr hModule);

    [DllImport(Kernel32BinaryName, SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr GetModuleHandle(string moduleName);

    [DllImport(Kernel32BinaryName, SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibrary(string lpFileName);


    [DllImport(Kernel32BinaryName, SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadPackagedLibrary(string lpFileName, int reserved=0);

    [DllImport(Kernel32BinaryName, SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);
#endif

    private static SystemDynamicLibrary s_instance;

    // "__Internal" is the name used for static linked libraries
    private const string DLLHBinaryName =
#if UNITY_WSA || UNITY_STANDALONE_WIN
        "DynamicLibraryLoaderHelper";
#else
        "__Internal";
#endif


    [DllImport(DLLHBinaryName)]
    private static extern IntPtr DLLH_create_context();

    [DllImport(DLLHBinaryName)]
    private static extern void DLLH_destroy_context(IntPtr context);

    [DllImport(DLLHBinaryName, SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern IntPtr DLLH_load_library_at_path(IntPtr ctx, string library_path);

    [DllImport(DLLHBinaryName)]
    private static extern bool DLLH_unload_library_at_path(IntPtr ctx, IntPtr library_handle);

    [DllImport(DLLHBinaryName, SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern IntPtr DLLH_load_function_with_name(IntPtr ctx, IntPtr library_handle, string function);

    private IntPtr DLLHContex;

    //-------------------------------------------------------------------------
    private SystemDynamicLibrary()
    {
#if !UNITY_EDITOR
        DLLHContex = DLLH_create_context();
#endif
    }

    //-------------------------------------------------------------------------
    static public SystemDynamicLibrary Instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = new SystemDynamicLibrary();
            }
            return s_instance;
        }
    }

    //-------------------------------------------------------------------------
    public IntPtr LoadLibraryAtPath(string libraryPath)
    {
#if UNITY_EDITOR_WIN
        return LoadLibrary(libraryPath);
#else
        return DLLH_load_library_at_path(DLLHContex, libraryPath);
#endif
    }

    //-------------------------------------------------------------------------
    public bool UnloadLibrary(IntPtr libraryHandle)
    {
#if UNITY_EDITOR_WIN
        return FreeLibrary(libraryHandle);
#else
        return DLLH_unload_library_at_path(DLLHContex, libraryHandle);
#endif
    }

    //-------------------------------------------------------------------------
    // TODO: evaluate if we can just use DLLH_load_function; it might make it
    // more difficult to iterate on the DLLH dll if the Unity Editor holds a lock
    // on the DLL
    public IntPtr LoadFunctionWithName(IntPtr libraryHandle, string functionName)
    {
#if UNITY_EDITOR_WIN
        return GetProcAddress(libraryHandle, functionName);
#else
        return DLLH_load_function_with_name(DLLHContex, libraryHandle, functionName);
#endif
    }
}
