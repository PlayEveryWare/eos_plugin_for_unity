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
    [DllImport("kernel32", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool FreeLibrary(IntPtr hModule);

    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr LoadLibrary(string lpFileName);

    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr GetModuleHandle(string moduleName);

    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true)]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);
}
