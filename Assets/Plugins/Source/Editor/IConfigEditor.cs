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
        /// <summary>
        /// Returns the human-readable label for the section of configuration.
        /// </summary>
        /// <returns>String representing the section this config controls.</returns>
        string GetLabel();

        /// <summary>
        /// Loads the config values from disk.
        /// </summary>
        void Read();

        /// <summary>
        /// Saves the configuration to disk.
        /// </summary>
        /// <param name="prettyPrint">Whether or not to format the JSON in a more human-readable manner.</param>
        void Save(bool prettyPrint);

        /// <summary>
        /// Used to render the configuration. It's important to note that despite the name, UnityEditor never
        /// directly calls this method, rather difference classes within this project may call it when they
        /// wish to display GUI controls to allow editing of the config values.
        /// </summary>
        void OnGUI();
    }

    /// <summary>
    /// Contains implementations of IConfigEditor that are common to all implementing classes.
    /// </summary>
    /// <typeparam name="T">Indented to be a type accepted by the templated class EOSConfigFile.</typeparam>
    public abstract class ConfigEditor<T> : IConfigEditor where T : IEmpty, ICloneableGeneric<T>, new()
    {
        private readonly string _configLabel;
        protected EOSConfigFile<T> configFile;

        protected ConfigEditor(string label, string file)
        {
            _configLabel = label;
            configFile = new EOSConfigFile<T>(Path.Combine("Assets", "StreamingAssets", "EOS", file));
        }

        public string GetLabel()
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
