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
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for allowing adding additional config files to the Config
    /// editor.
    /// </summary>
    public interface IConfigEditor : IDisposable
    {
        /// <summary>
        /// Returns the human-readable labelText for the section of
        /// configuration.
        /// </summary>
        /// <returns>
        /// String representing the section this config controls.
        /// </returns>
        string GetLabelText();

        /// <summary>
        /// Expands the ConfigEditor.
        /// </summary>
        public void Expand();

        /// <summary>
        /// Collapses the ConfigEditor.
        /// </summary>
        public void Collapse();

        /// <summary>
        /// Loads the config values from disk asynchronously.
        /// </summary>
        Task LoadAsync();

        /// <summary>
        /// Loads the config values from disk synchronously.
        /// </summary>
        void Load();

        /// <summary>
        /// Saves the configuration to disk.
        /// </summary>
        /// <param name="prettyPrint">
        /// Whether or not to format the JSON in a more human-readable manner.
        /// </param>
        Task Save(bool prettyPrint = true);

        /// <summary>
        /// Render the editor for the configuration values.
        /// </summary>
        Task RenderAsync();
    }
}