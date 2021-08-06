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

﻿//#define ENABLE_DLLHANDLE_PRINT

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

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
    private static string GetPackageName()
    {
        return "com.playeveryware.eos";
    }

    //-------------------------------------------------------------------------
    public static List<string> GetPathsToPlugins()
    {
        string uwpPluginsPath = Path.Combine(Application.streamingAssetsPath, "..", "..");
        string pluginsPath = (Application.dataPath + "\\Plugins\\").Replace('/', '\\');
        string packagedPluginPath = Path.GetFullPath(Path.Combine("Packages", GetPackageName(), "Runtime"));
        var pluginPaths = new List<string>();

        if(Directory.Exists(pluginsPath))
        {
            pluginPaths.Add(pluginsPath);
        }
        if(Directory.Exists(packagedPluginPath))
        {
            pluginPaths.Add(packagedPluginPath);
        }

        if (Directory.Exists(uwpPluginsPath))
        {
            pluginPaths.Add(uwpPluginsPath);
        }

        return pluginPaths;
    }

    //-------------------------------------------------------------------------
    public static string GetVersionForLibrary(string libraryName)
    {
        List<string> pluginPaths = GetPathsToPlugins();
        string ext = ".dll";

        //TODO: change it to take the platform into consideration
        //TODO: probably make this more generic?

        foreach (string pluginPath in pluginPaths)
        {
            foreach (var filesystemEntry in Directory.EnumerateFileSystemEntries(pluginPath, libraryName + ext, SearchOption.AllDirectories))
            {
                FileVersionInfo info = FileVersionInfo.GetVersionInfo(filesystemEntry);
                print("Found : " + filesystemEntry);
                if (info != null)
                {
                    return string.Format("{0}.{1}.{2}", info.FileMajorPart, info.FileMinorPart, info.FileBuildPart);
                }
            }
        }
        return null;
    }

    //-------------------------------------------------------------------------
#if UNITY_WSA
    private static DLLHandle LoadDynamicLibraryForUWP(string libraryName)
    {
        print("UWP library load");
        IntPtr libraryHandle = SystemDynamicLibrary.Instance.LoadLibraryAtPath(libraryName);
        if (libraryHandle != IntPtr.Zero)
        {
            print("found library");
            return new DLLHandle(libraryHandle);
        }
        return null;
    }
#endif

    //-------------------------------------------------------------------------
    public static DLLHandle LoadDynamicLibrary(string libraryName)
    {
        print("Loading Library " + libraryName);
        List<string> pluginPaths = GetPathsToPlugins();
        string ext =
#if UNITY_ANDROID
            ".so";
#else
            ".dll";
#endif

        //TODO: change it to take the platform into consideration
        //TODO: probably make this more generic?
        foreach (string pluginPath in pluginPaths)
        {
            foreach (var filesystemEntry in Directory.EnumerateFileSystemEntries(pluginPath, libraryName + ext, SearchOption.AllDirectories))
            {
                IntPtr libraryHandle = SystemDynamicLibrary.Instance.LoadLibraryAtPath(filesystemEntry);
                print("Trying to load with entry " + filesystemEntry);

                if (libraryHandle != IntPtr.Zero)
                {
                    print("found library");
                    return new DLLHandle(libraryHandle);
                }
                else
                {
                    throw new System.ComponentModel.Win32Exception();
                }
            }
        }
        print("Library not found");
        return null;
    }

    //-------------------------------------------------------------------------
    public DLLHandle(IntPtr intPtr, bool value = true) : base(intPtr, true)
    {
        print("Creating a dll handle");
        SetHandle(intPtr);
    }

    //-------------------------------------------------------------------------
    protected override bool ReleaseHandle()
    {
        if(handle == IntPtr.Zero)
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
    public void ConfigureFromLibraryDelegateFieldOnClassWithFunctionName(Type clazz, Type delegateType, string functionName)
    {
        ConfigureFromLibraryDelegateFieldOnClassWithFunctionName(handle, clazz, delegateType, functionName);
    }

    //-------------------------------------------------------------------------
    // TODO better name
    private static void ConfigureFromLibraryDelegateFieldOnClassWithFunctionName(IntPtr libraryHandle, Type clazz, Type delegateType, string functionName)
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