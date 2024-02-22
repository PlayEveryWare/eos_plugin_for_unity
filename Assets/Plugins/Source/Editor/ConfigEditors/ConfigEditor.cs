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
    using Settings;
    using System.IO;

    /// <summary>
    /// Contains implementations of IConfigEditor that are common to all implementing classes.
    /// </summary>
    /// <typeparam name="T">Indented to be a type accepted by the templated class EOSConfigFile.</typeparam>
    public abstract class ConfigEditor<T> : IConfigEditor where T : Config, new()
    {
        private static readonly string ConfigDirectory = Path.Combine("Assets", "StreamingAssets", "EOS");

        private readonly string _labelText;
        protected ConfigHandler<T> ConfigHandler;

        protected ConfigEditor(string labelText, string config_filename)
        {
            _labelText = labelText;
            ConfigHandler = new ConfigHandler<T>(Path.Combine(ConfigDirectory, config_filename));
        }

        public string GetLabelText()
        {
            return _labelText;
        }

        public ConfigHandler<T> GetConfig()
        {
            return ConfigHandler;
        }

        public void Load()
        {
            ConfigHandler.Read();
        }

        public bool HasUnsavedChanges()
        {
            return ConfigHandler.HasChanges;
        }

        public void Save(bool prettyPrint)
        {
            ConfigHandler.Write(prettyPrint);
        }

        public abstract void RenderContents();

        public void Render()
        {
            if (ConfigHandler == null)
            {
                Load();
            }

            RenderContents();
        }

    }
}