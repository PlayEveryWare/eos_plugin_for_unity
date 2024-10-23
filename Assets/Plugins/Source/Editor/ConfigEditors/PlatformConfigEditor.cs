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
namespace PlayEveryWare.EpicOnlineServices.Editor
{
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Utility;

    /// <summary>
    /// Contains implementations of IConfigEditor that are common to all
    /// ConfigEditors that represent the configuration options for a specific
    /// platform.
    /// </summary>
    /// <typeparam name="T">Intended to be a type accepted by the templated class ConfigHandler.</typeparam>
    public class PlatformConfigEditor<T> : ConfigEditor<T>, IPlatformConfigEditor where T : PlatformConfig
    {
        public PlatformConfigEditor()
        {
            Load();

            // The label should always be the full name of the platform.
            _labelText = PlatformManager.GetFullName(config.Platform);
        }

        public bool IsPlatformAvailable()
        {
            return PlatformManager.GetAvailableBuildTargets().Contains(config.Platform);
        }

        public Texture GetPlatformIconTexture()
        {
            return PlatformManager.GetPlatformIcon(config.Platform);
        }

        public override sealed void RenderContents()
        {
            GUIEditorUtility.RenderInputs(ref config);
        }
    }
}