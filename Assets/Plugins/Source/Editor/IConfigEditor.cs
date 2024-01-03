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

namespace PlayEveryWare.EpicOnlineServices
{
    using System.IO;

    
    // Interface for allowing adding additional config files to the Config editor
    public interface IConfigEditor
    {
        string GetName();

        void Read();

        void Save(bool prettyPrint);

        void OnGUI();
    }

    public abstract class ConfigEditor<T> : IConfigEditor where T : IEmpty, ICloneableGeneric<T>, new()
    {
        private readonly string _configLabel;
        protected EOSConfigFile<T> configFile;

        protected ConfigEditor(string label, string file)
        {
            _configLabel = label;
            configFile = new EOSConfigFile<T>(Path.Combine("Assets", "StreamingAssets", "EOS", file));
        }

        public string GetName()
        {
            return _configLabel;
        }

        public EOSConfigFile<T> GetConfig()
        {
            return configFile;
        }

        public void Read()
        {
            configFile.LoadConfigFromDisk();
        }

        public void Save(bool prettyPrint)
        {
            configFile.SaveToJSONConfig(prettyPrint);
        }

        public abstract void OnGUI();

    }
}
