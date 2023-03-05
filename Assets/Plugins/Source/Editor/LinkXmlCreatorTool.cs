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

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Playeveryware.Editor
{
    public static class LinkXmlCreatorTool
    {
        [MenuItem("Tools/Create link.xml")]
        static void CreateLinkXml()
        {
            var linkSourceFilePath = Path.Combine("Packages", EOSPackageInfo.GetPackageName(), "Editor", "link.xml");
            var linkDestPath = Path.Combine("Assets", "EOS");
            var linkDestFilePath = Path.Combine(linkDestPath, "link.xml");
            if (File.Exists(linkSourceFilePath))
            {
                if (!Directory.Exists(linkDestPath))
                {
                    Directory.CreateDirectory(linkDestPath);
                }

                //copy link.xml if the destination copy doesn't exist or the user allows an overwrite
                if (!File.Exists(linkDestFilePath) || EditorUtility.DisplayDialog("File Exists", string.Format("The file '{0}' already exists. Overwrite?", linkDestFilePath), "Overwrite", "Cancel"))
                {
                    File.Copy(linkSourceFilePath, linkDestFilePath);
                    AssetDatabase.Refresh();
                }
            }
            else
            {
                Debug.LogError("link.xml not found");
            }
        }

        //disable menu item if not running within a UPM package
        [MenuItem("Tools/Create link.xml", true)]
        static bool ValidateCreateLinkXml()
        {
            return Directory.Exists(Path.Combine("Packages", EOSPackageInfo.GetPackageName(), "Editor"));
        }
    }
}