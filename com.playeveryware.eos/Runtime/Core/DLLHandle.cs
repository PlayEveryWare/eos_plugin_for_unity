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

#if !EOS_DISABLE

// Uncomment the following line to enable debug printing from within DLLHandle.
//#define ENABLE_DLLHANDLE_PRINT

namespace PlayEveryWare.EpicOnlineServices
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using Utility;

    public class DLLHandle : SafeHandle
    {
        public override bool IsInvalid => handle == IntPtr.Zero;

        //-------------------------------------------------------------------------
        [Conditional("ENABLE_DLLHANDLE_PRINT")]
        private static void print(string toPrint)
        {
            UnityEngine.Debug.Log(toPrint);
        }

        //-------------------------------------------------------------------------
        public static List<string> GetPathsToPlugins()
        {
            string pluginsPath = FileSystemUtility.CombinePaths(Application.dataPath, "Plugins");
            string packagedPluginPath =
                FileSystemUtility.GetFullPath(FileSystemUtility.CombinePaths("Packages", EOSPackageInfo.PackageName, "Runtime"));
            var pluginPaths = new List<string>();

            pluginPaths.Add(pluginsPath);
            pluginPaths.Add(packagedPluginPath);

            if (EOSManagerPlatformSpecificsSingleton.Instance != null)
            {
                EOSManagerPlatformSpecificsSingleton.Instance.AddPluginSearchPaths(ref pluginPaths);
            }

            for (int i = pluginPaths.Count - 1; i >= 0; --i)
            {
                string value = pluginPaths[i];
                print("print " + value);
            }

            // Do a validation check after giving the 
            // EOSManagerPlatformSpecific a change to modify the list
            for (int i = pluginPaths.Count - 1; i >= 0; --i)
            {
                string value = pluginPaths[i];
                print("Evaluating " + value);
                if (!FileSystemUtility.DirectoryExists(value))
                {
                    pluginPaths.RemoveAt(i);
                }
            }

            return pluginPaths;
        }

        //-------------------------------------------------------------------------
        public static string GetVersionForLibrary(string libraryName)
        {
            FileVersionInfo info = GetLibraryVersionInfo(libraryName);

            return null == info ? null : $"{info.FileMajorPart}.{info.FileMinorPart}.{info.FileBuildPart}";
        }

        //-------------------------------------------------------------------------
        public static string GetProductVersionForLibrary(string libraryName)
        {
            FileVersionInfo info = GetLibraryVersionInfo(libraryName);

            return info?.ProductVersion;
        }

        private static FileVersionInfo GetLibraryVersionInfo(string libraryName)
        {
            string libraryPath = GetPathForLibrary(libraryName);

            return null == libraryPath ? null : FileVersionInfo.GetVersionInfo(libraryPath);
        }

        //-------------------------------------------------------------------------
        public static string GetPathForLibrary(string libraryName)
        {
            List<string> pluginPaths = GetPathsToPlugins();

            string extension = EOSManagerPlatformSpecificsSingleton.Instance != null
                ? EOSManagerPlatformSpecificsSingleton.Instance.GetDynamicLibraryExtension()
                : ".dll";

            foreach (string pluginPath in pluginPaths)
            {
                foreach (string entry in FileSystemUtility.GetFileSystemEntries(pluginPath, libraryName + extension))
                {
                    return entry;
                }
            }

            return null;
        }

        //-------------------------------------------------------------------------
        public static DLLHandle LoadDynamicLibrary(string libraryName)
        {
            print("Loading Library " + libraryName);

            string libraryPath = GetPathForLibrary(libraryName);

            if (null == libraryPath)
            {
                return null;
            }

            print("Trying to load with entry " + libraryPath);
            IntPtr libraryHandle = SystemDynamicLibrary.Instance.LoadLibraryAtPath(libraryPath);

            if (IntPtr.Zero == libraryHandle)
            {
                throw new System.ComponentModel.Win32Exception($"Could not load dynamic library from \"{libraryPath}\".");
            }
            
            return new DLLHandle(libraryHandle);
        }

        //-------------------------------------------------------------------------
        public DLLHandle(IntPtr intPtr) : base(intPtr, true)
        {
            print("Creating a dll handle");
            SetHandle(intPtr);
        }

        //-------------------------------------------------------------------------
        protected override bool ReleaseHandle()
        {
            if (handle == IntPtr.Zero)
            {
                return true;
            }

            bool didUnload = true;
#if !UNITY_EDITOR
            didUnload = SystemDynamicLibrary.Instance.UnloadLibrary(handle);
            print("Unloading a Dll with result : " + didUnload);
#endif
            SetHandle(IntPtr.Zero);
            return didUnload;
        }

        //-------------------------------------------------------------------------
        public Delegate LoadFunctionAsDelegate(Type functionType, string functionName)
        {
            return LoadFunctionAsDelegate(handle, functionType, functionName);
        }

        //-------------------------------------------------------------------------
        public System.IntPtr LoadFunctionAsIntPtr(string functionName)
        {
            IntPtr functionPointer = SystemDynamicLibrary.Instance.LoadFunctionWithName(handle, functionName);
            return functionPointer;
        }

        //-------------------------------------------------------------------------
        public void ConfigureFromLibraryDelegateFieldOnClassWithFunctionName(Type clazz, Type delegateType,
            string functionName)
        {
            ConfigureFromLibraryDelegateFieldOnClassWithFunctionName(handle, clazz, delegateType, functionName);
        }

        //-------------------------------------------------------------------------
        // TODO better name
        private static void ConfigureFromLibraryDelegateFieldOnClassWithFunctionName(IntPtr libraryHandle, Type clazz,
            Type delegateType, string functionName)
        {
            var aDelegate = LoadFunctionAsDelegate(libraryHandle, delegateType, functionName);
            //print("Delegate found is : " + aDelegate);
            var field = clazz.GetField(functionName);
            field.SetValue(null, aDelegate);
        }

        //-------------------------------------------------------------------------
        static public Delegate LoadFunctionAsDelegate(IntPtr libraryHandle, Type functionType, string functionName)
        {
            print("Attempt to load " + functionName);
            if (libraryHandle == IntPtr.Zero)
            {
                throw new Exception("libraryHandle is null");
            }

            if (functionType == null)
            {
                throw new Exception("null function type?");
            }

            IntPtr functionPointer = SystemDynamicLibrary.Instance.LoadFunctionWithName(libraryHandle, functionName);
            if (functionPointer == IntPtr.Zero)
            {
                throw new Exception("Function not found: " + functionName);
            }

            return Marshal.GetDelegateForFunctionPointer(functionPointer, functionType);
        }
    }
}
#endif