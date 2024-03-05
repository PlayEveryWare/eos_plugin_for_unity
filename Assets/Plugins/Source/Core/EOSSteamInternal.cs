/*
* Copyright (c) 2024 PlayEveryWare
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

namespace PlayEveryWare.EpicOnlineServices
{
    using System.Runtime.InteropServices;
    using System.Text;
    using System;

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    struct EOSSteamInternal : IDisposable
    {
        public int ApiVersion;
        public IntPtr m_OverrideLibraryPath;

        public string OverrideLibraryPath
        {
            set
            {
                if (m_OverrideLibraryPath != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(m_OverrideLibraryPath);
                }

                if (value == null)
                {
                    m_OverrideLibraryPath = IntPtr.Zero;
                    return;
                }

                // Manually copy of the C# string to an UTF-8 C-String
                byte[] valueAsBytes = Encoding.UTF8.GetBytes(value);
                int valueSizeInBytes = valueAsBytes.Length;
                m_OverrideLibraryPath = Marshal.AllocCoTaskMem(valueSizeInBytes + 1);
                Marshal.Copy(valueAsBytes, 0, m_OverrideLibraryPath, valueSizeInBytes);
                // NULL terminate the string
                Marshal.WriteByte(m_OverrideLibraryPath, valueSizeInBytes, 0);

            }
        }

        public void Dispose()
        {
            if (m_OverrideLibraryPath != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(m_OverrideLibraryPath);
            }
        }
    }
}