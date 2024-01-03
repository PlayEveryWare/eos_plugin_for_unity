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
    public interface IPlatformSpecificConfigEditor
    {
        string GetNameForMenu();

        void Read();

        void Save(bool prettyPrint);

        void OnGUI();
    }

    public abstract class PlatformSpecificConfigEditor<T> : IPlatformSpecificConfigEditor where T : IEmpty, ICloneableGeneric<T>, new()
    {
        protected readonly string configFilePath;
        protected readonly string configName;
        protected EOSConfigFile<T> configFile;

        protected PlatformSpecificConfigEditor(string name, string file)
        {
            configName = name;
            configFilePath = file;
        }

        public string GetNameForMenu()
        {
            return configName;
        }

        public void Read()
        {
            string filepath = Path.Combine("Assets", "StreamingAssets", "EOS", configFilePath);
            configFile = new EOSConfigFile<T>(filepath);
            configFile.LoadConfigFromDisk();
        }

        public void Save(bool prettyPrint)
        {
            configFile.SaveToJSONConfig(prettyPrint);
        }

        public abstract void OnGUI();

    }
}
