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

namespace PlayEveryWare.EpicOnlineServices.Editor.Config
{
    using EpicOnlineServices.Utility;
    using System.IO;
    using System.Threading.Tasks;
    using Config = EpicOnlineServices.Config;

    /// <summary>
    /// Used for configurations that are editor-only.
    /// Configurations are serialized into JSON strings, and then added to EditorPrefs
    /// using their filename as the key.
    /// </summary>
    public abstract class EditorConfig : Config
    {
        protected EditorConfig(string filename) : base(filename, Path.Combine(FileSystemUtility.GetProjectPath(), "etc/config/")) { }

        // NOTE: This compiler block is here because the base class "Config" has
        //       the WriteAsync function surrounded by the same conditional.
#if UNITY_EDITOR
        // Overridden functionality changes the default parameter value for updateAssetDatabase, because EditorConfig
        // should not be anywhere within Assets.
        public override async Task WriteAsync(bool prettyPrint = true, bool updateAssetDatabase = false)
        {
            // Override the base function
            await base.WriteAsync(prettyPrint, updateAssetDatabase);
        }
#endif
    }
    
}