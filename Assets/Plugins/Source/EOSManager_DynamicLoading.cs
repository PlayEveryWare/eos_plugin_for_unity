//#define USE_MANUAL_METHOD_LOADING

#if !UNITY_EDITOR && !UNITY_SWITCH && !UNITY_ANDROID && !UNITY_PS4
#define USE_EOS_GFX_PLUGIN_NATIVE_RENDER
#endif

#if UNITY_64
#define PLATFORM_64BITS
#elif (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
#define PLATFORM_32BITS
#endif


#if UNITY_EDITOR
// Define this if using the new version of the EOS 1.12
#define EOS_DYNAMIC_BINDINGS
#endif


using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System;

using Epic.OnlineServices.Platform;
using Epic.OnlineServices;

namespace PlayEveryWare.EpicOnlineServices
{
    public partial class EOSManager
    {
        public partial class EOSSingleton
        {

            static private PlatformInterface s_eosPlatformInterface;

            public const string EOSBinaryName = Epic.OnlineServices.Config.LibraryName;

#if USE_EOS_GFX_PLUGIN_NATIVE_RENDER
            public const string GfxPluginNativeRenderPath = 
#if UNITY_STANDALONE_OSX
                "GfxPluginNativeRender-macOS";
#elif UNITY_STANDALONE_WIN && PLATFORM_64BITS
            "GfxPluginNativeRender-x64";
#elif UNITY_STANDALONE_WIN && PLATFORM_32BITS
            "GfxPluginNativeRender-x86";
#else
#error Unknown platform
            "GfxPluginNativeRender-unknown";
#endif

            [DllImport(GfxPluginNativeRenderPath)]
            static extern void UnloadEOS();

            [DllImport(GfxPluginNativeRenderPath,CallingConvention = CallingConvention.StdCall)]
            static extern IntPtr EOS_GetPlatformInterface();
#endif

            static void NativeCallToUnloadEOS()
            {
#if USE_EOS_GFX_PLUGIN_NATIVE_RENDER
                UnloadEOS();
#endif
            }

            //-------------------------------------------------------------------------
            public PlatformInterface GetEOSPlatformInterface()
            {
#if USE_EOS_GFX_PLUGIN_NATIVE_RENDER
                if (s_eosPlatformInterface == null)
                {
                    if(EOS_GetPlatformInterface() == IntPtr.Zero)
                    {
                        throw new Exception("bad eos platform ");
                    }
                    SetEOSPlatformInterface(new Epic.OnlineServices.Platform.PlatformInterface(EOS_GetPlatformInterface()));
                }
#endif
                return s_eosPlatformInterface;

            }

            //-------------------------------------------------------------------------
            void SetEOSPlatformInterface(PlatformInterface platformInterface)
            {
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

            static public void LoadDelegatesWithEOSBindingAPI()
            {
#if EOS_DYNAMIC_BINDINGS
                var eosLibraryHandle = LoadDynamicLibrary(EOSBinaryName);

                Epic.OnlineServices.Bindings.Hook<DLLHandle>(eosLibraryHandle, (DLLHandle handle, string functionName) => {
                        return handle.LoadFunctionAsIntPtr(functionName);
                        });
#endif
            }
            //-------------------------------------------------------------------------
            // Using runtime reflection, hook up things
            static public void LoadDelegatesWithReflection()
            {
                var selectedAssemblies = new List<Assembly>();
                var eosLibraryHandle = LoadDynamicLibrary(EOSBinaryName);
                selectedAssemblies.Add(typeof(PlatformInterface).Assembly);

                foreach(var assembly in selectedAssemblies)
                {
                    foreach(var clazz in assembly.GetTypes())
                    {
                        if (clazz.Namespace == null)
                        {
                            continue;
                        }
                        //print("Looking at this class " + clazz.FullName);
                        if(!clazz.Namespace.StartsWith("Epic"))
                        {
                            continue;
                        }

                        //TODO figure out if can actually modify binding at runtime between a method impl and the c function 
                        // so I don't have to use delegates. Or don't worry about it because maybe it doesn't impact perf that much?
                        //foreach (var method in clazz.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                        //{
                        //    if(method.IsDefined(typeof(DllImportAttribute)))
                        //    {

                        //    }
                        //}

                        // This works due to an assumed naming convention in the EOS Library that was added for this plugin
                        foreach(var member in clazz.GetMembers(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                        {
                            FieldInfo fieldInfo = clazz.GetField(member.Name);
                            if (fieldInfo == null)
                            {
                                continue;
                            }

                            if (fieldInfo.FieldType.Name.EndsWith("_delegate"))
                            {
                                var delegateType = fieldInfo.FieldType;
                                var functionNameAsString = member.Name.Replace("_delegate", "");
                                eosLibraryHandle.ConfigureFromLibraryDelegateFieldOnClassWithFunctionName(clazz, delegateType, functionNameAsString);
                            }
                        }
                    }
                }
            }

            //-------------------------------------------------------------------------
            // At the moment, this isn't supported on Consoles.
            static private void LoadDelegatesByHand()
            {
#if UNITY_EDITOR && !UNITY_SWITCH && USE_MANUAL_METHOD_LOADING
                var eosLibraryHandle = LoadDynamicLibrary(Epic.OnlineServices.Config.BinaryName);

                var eosPlatformType = typeof(PlatformInterface);

                eosLibraryHandle.ConfigureFromLibraryDelegateFieldOnClassWithFunctionName(eosPlatformType, typeof(PlatformInterface.EOS_Initialize_delegate), "EOS_Initialize");
                eosLibraryHandle.ConfigureFromLibraryDelegateFieldOnClassWithFunctionName(eosPlatformType, typeof(PlatformInterface.EOS_Platform_Create_delegate), "EOS_Platform_Create");
                eosLibraryHandle.ConfigureFromLibraryDelegateFieldOnClassWithFunctionName(eosPlatformType, typeof(PlatformInterface.EOS_Platform_Tick_delegate), "EOS_Platform_Tick");
                eosLibraryHandle.ConfigureFromLibraryDelegateFieldOnClassWithFunctionName(eosPlatformType, typeof(PlatformInterface.EOS_Platform_GetAuthInterface_delegate), "EOS_Platform_GetAuthInterface");
                eosLibraryHandle.ConfigureFromLibraryDelegateFieldOnClassWithFunctionName(eosPlatformType, typeof(PlatformInterface.EOS_Platform_Release_delegate), "EOS_Platform_Release");
                eosLibraryHandle.ConfigureFromLibraryDelegateFieldOnClassWithFunctionName(eosPlatformType, typeof(PlatformInterface.EOS_Shutdown_delegate), "EOS_Shutdown");
                eosLibraryHandle.ConfigureFromLibraryDelegateFieldOnClassWithFunctionName(eosPlatformType, typeof(PlatformInterface.EOS_Platform_GetConnectInterface_delegate), "EOS_Platform_GetConnectInterface");

                var eosHelper = typeof(Helper);
                eosLibraryHandle.ConfigureFromLibraryDelegateFieldOnClassWithFunctionName(eosHelper, typeof(Helper.EOS_EResult_IsOperationComplete_delegate), "EOS_EResult_IsOperationComplete");


                var eosLoggingType = typeof(LoggingInterface);
                eosLibraryHandle.ConfigureFromLibraryDelegateFieldOnClassWithFunctionName(eosLoggingType, typeof(LoggingInterface.EOS_Logging_SetCallback_delegate), "EOS_Logging_SetCallback");

                var eosAuthType = typeof(Epic.OnlineServices.Auth.AuthInterface);
                eosLibraryHandle.ConfigureFromLibraryDelegateFieldOnClassWithFunctionName(eosAuthType, typeof(Epic.OnlineServices.Auth.AuthInterface.EOS_Auth_Login_delegate), "EOS_Auth_Login");
                eosLibraryHandle.ConfigureFromLibraryDelegateFieldOnClassWithFunctionName(eosAuthType, typeof(Epic.OnlineServices.Auth.AuthInterface.EOS_Auth_Logout_delegate), "EOS_Auth_Logout");
#endif
            }

            static bool loadEOSWithReflection
#if UNITY_EDITOR
                = true;
#else
            = false;
#endif
            //-------------------------------------------------------------------------
            // At the moment this only works on Windows.
            static private void ForceUnloadEOSLibrary()
            {
#if EOS_DYNAMIC_BINDINGS
                Epic.OnlineServices.Bindings.Unhook();
#endif

                IntPtr existingHandle;
                do
                {
                    existingHandle = SystemDynamicLibrary.GetModuleHandle(EOSBinaryName);
                    if (existingHandle != IntPtr.Zero)
                    {
                        GC.WaitForPendingFinalizers();
                        SystemDynamicLibrary.FreeLibrary(existingHandle);
                    }

                } while (IntPtr.Zero != existingHandle);
            }

            //-------------------------------------------------------------------------
            static public void LoadEOSLibraries()
            {
                if (loadEOSWithReflection)
                {
#if EOS_DYNAMIC_BINDINGS
                    LoadDelegatesWithEOSBindingAPI();
#else
                    LoadDelegatesWithReflection();
#endif
                    return;
                }

#if UNITY_EDITOR
                LoadDelegatesByHand();
#endif
            }
        }
    }
}
