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
    using Config;
    using Utility;

    public class LibraryBuildConfigEditor : ConfigEditor<LibraryBuildConfig>
    {
        public LibraryBuildConfigEditor() : 
            base("Platform Library Build Settings")
        { }

        public override void RenderContents()
        {
            string msbuildPath = (config.msbuildPath);
            string makePath = (config.makePath);
            bool msbuildDebug = config.msbuildDebug;
            GUIEditorUtility.AssigningPath("MSBuild path", ref msbuildPath, "Select MSBuild", labelWidth: 80);
            GUIEditorUtility.AssigningPath("Make path", ref makePath, "Select make", labelWidth: 80);
            GUIEditorUtility.AssigningBoolField("Use debug config for MSBuild", ref msbuildDebug, labelWidth: 180);
            config.msbuildPath = msbuildPath;
            config.makePath = makePath;
            config.msbuildDebug = msbuildDebug;
        }
    }
}