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

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR && !UNITY_IOS && !UNITY_STANDALONE_OSX
#define USE_EOS_GFX_PLUGIN_NATIVE_RENDER
#endif

#if UNITY_EDITOR
#define EOS_DYNAMIC_BINDINGS
#endif


using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System;

#if !EOS_DISABLE
using Epic.OnlineServices.Platform;
using Epic.OnlineServices;
#endif

namespace PlayEveryWare.EpicOnlineServices
{
    public partial class EOSManager
    {
        /// <summary>
        /// Singleton design pattern implementation for <c>EOSManager</c>.
        /// </summary>
        public partial class EOSSingleton
        {
#if !EOS_DISABLE
            static private PlatformInterface s_eosPlatformInterface;

            public const string EOSBinaryName = Epic.OnlineServices.Config.LibraryName;

#if USE_EOS_GFX_PLUGIN_NATIVE_RENDER
            public const string GfxPluginNativeRenderPath =
#if UNITY_STANDALONE_OSX
                "GfxPluginNativeRender-macOS";
#elif UNITY_STANDALONE_WIN
#if UNITY_64
                "GfxPluginNativeRender-x64";
#else
                "GfxPluginNativeRender-x86";
#endif // UNITY_64
#else
                #error Unknown platform
                "GfxPluginNativeRender-unknown";
#endif

            [DllImport(GfxPluginNativeRenderPath,CallingConvention = CallingConvention.StdCall)]
            static extern IntPtr EOS_GetPlatformInterface();

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            delegate void PrintDelegateType(string str);
            [DllImport(GfxPluginNativeRenderPath,CallingConvention = CallingConvention.StdCall)]
            static extern void global_log_flush_with_function(IntPtr ptr);
#endif


            //-------------------------------------------------------------------------
            public PlatformInterface GetEOSPlatformInterface()
            {
#if USE_EOS_GFX_PLUGIN_NATIVE_RENDER
                if (s_eosPlatformInterface == null && s_state != EOSState.Shutdown)
                {
                    // Try to log any messages stored when starting up the Plugin.
                    IntPtr logErrorFunctionPointer = Marshal.GetFunctionPointerForDelegate(new PrintDelegateType(SimplePrintStringCallback));
                    SimplePrintStringCallback("Start of Early EOS LOG:");
                    global_log_flush_with_function(logErrorFunctionPointer);
                    SimplePrintStringCallback("End of Early EOS LOG");

                    if (EOS_GetPlatformInterface() == IntPtr.Zero)
                    {
                        throw new Exception("NULL EOS Platform returned by native code: issue probably occurred in GFX Plugin!");
                    }
                    SetEOSPlatformInterface(new Epic.OnlineServices.Platform.PlatformInterface(EOS_GetPlatformInterface()));
                }
#endif
                return s_eosPlatformInterface;

            }

            //-------------------------------------------------------------------------
            void SetEOSPlatformInterface(PlatformInterface platformInterface)
            {
                if (platformInterface != null)
                {
                    s_state = EOSState.Running;
                }
                s_eosPlatformInterface = platformInterface;
            }


            //-------------------------------------------------------------------------
            static private void AddAllAssembliesInCurrentDomain(List<Assembly> list)
            {
                var appAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in appAssemblies)
                {
                    //var assemblyBasename = Path.GetFileNameWithoutExtension(assembly.Location);

                    list.Add(assembly);
                }
            }

            //-------------------------------------------------------------------------
            // This currently only works on windows
            static private Dictionary<string, DLLHandle> LoadedDLLs = new Dictionary<string, DLLHandle>();
            static public DLLHandle LoadDynamicLibrary(string libraryName)
            {
                if(LoadedDLLs.ContainsKey(libraryName))
                {
                    print("Found existing handle for " + libraryName);
                    return LoadedDLLs[libraryName];
                }
                else
                {
                    var libraryHandle = DLLHandle.LoadDynamicLibrary(libraryName);
                    LoadedDLLs[libraryName] = libraryHandle;
                    return libraryHandle;
                }
            }

            //-------------------------------------------------------------------------
            // This doesn't do anything smart to make sure the delegates aren't being used, so 
            // be mindful to only do this at the very end of the app usage
            static public void UnloadAllLibraries()
            {
                foreach(var entry in LoadedDLLs)
                {
                    entry.Value.Dispose();
                }
                LoadedDLLs.Clear();
                LoadedDLLs = new Dictionary<string, DLLHandle>();
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
            }

            //-------------------------------------------------------------------------
            static public void LoadDelegatesWithEOSBindingAPI()
            {
#if EOS_DYNAMIC_BINDINGS
                print($"Loading EOS binary {EOSBinaryName}");
                var eosLibraryHandle = LoadDynamicLibrary(EOSBinaryName);

                Epic.OnlineServices.Bindings.Hook<DLLHandle>(eosLibraryHandle, (DLLHandle handle, string functionName) => {
                // TODO: Add conditions for all flags (unless OSX is the only one that's weird?)
#if UNITY_EDITOR_OSX
                    return handle.LoadFunctionAsIntPtr(functionName.Trim('_'));
#else
                    return handle.LoadFunctionAsIntPtr(functionName);
#endif
                 });

                EOSManagerPlatformSpecificsSingleton.Instance?.LoadDelegatesWithEOSBindingAPI();
#endif
                }

            //-------------------------------------------------------------------------
            // At the moment this only works on Windows.
            static private void ForceUnloadEOSLibrary()
            {
#if EOS_DYNAMIC_BINDINGS
                Epic.OnlineServices.Bindings.Unhook();
#endif

#if UNITY_EDITOR
                IntPtr existingHandle;
                int timeout = 50;
                do
                {
                    existingHandle = SystemDynamicLibrary.GetHandleForModule(EOSBinaryName);
                    if (existingHandle != IntPtr.Zero)
                    {
                        GC.WaitForPendingFinalizers();
                        SystemDynamicLibrary.UnloadLibraryInEditor(existingHandle);
                    }
                    timeout--;
                } while (IntPtr.Zero != existingHandle && timeout > 0);

                if (IntPtr.Zero != existingHandle)
                {
                    UnityEngine.Debug.LogWarning("Free Library { " + EOSBinaryName + " }:Timeout");
                }
#endif
            }

            //-------------------------------------------------------------------------
            static public void LoadEOSLibraries()
            {
#if EOS_DYNAMIC_BINDINGS
                    LoadDelegatesWithEOSBindingAPI();
#endif
                    return;
            }
#endif
        }
    }
}
