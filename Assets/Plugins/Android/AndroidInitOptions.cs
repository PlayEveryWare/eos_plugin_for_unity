using System;
using System.Runtime.InteropServices;

using JavaVM = System.IntPtr;

//-------------------------------------------------------------------------
// This is just an C# version of  the native EOS android init options
[StructLayout(LayoutKind.Sequential, Pack = 8)]
public class EOSAndroidInitializeOptions : IDisposable
{
    public Int32 ApiVersion;

    /** JNI's Java VM */
    public JavaVM VM;

    /** Full internal directory path. Can be null */
    public IntPtr OptionalInternalDirectory;
    /** Full external directory path. Can be null */
    public IntPtr OptionalExternalDirectory;

    public void Dispose()
    {
    }
}
