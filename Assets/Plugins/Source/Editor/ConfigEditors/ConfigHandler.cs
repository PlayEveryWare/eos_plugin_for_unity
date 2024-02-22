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

using System.IO;
using UnityEditor;
using UnityEngine;

namespace PlayEveryWare.EpicOnlineServices.Editor
{
    /// <summary>
    /// Used to represent a "handle" to the config data, thus delegating all the IO work here.
    /// </summary>
    /// <typeparam name="T">The Config being represented</typeparam>
    public class ConfigHandler<T> where T : EpicOnlineServices.Config, new()
    {
        /// <summary>
        /// Fully-qualified path to the file that contains the configuration values.
        /// </summary>
        private string _configFilePath;

        /// <summary>
        /// The Config data as it exists on the disk.
        /// </summary>
        private T _dataOnDisk;

        /// <summary>
        /// The Config data as it exists in memory.
        /// </summary>
        public T Data;

        /// <summary>
        /// Creates a new ConfigHandle to represent the manipulation of the data at the given filepath.
        /// </summary>
        /// <param name="filepath">Fully-qualified path to the file containing the configuration values.</param>
        public ConfigHandler(string filepath)
        {
            _configFilePath = filepath;
        }

        /// <summary>
        /// Reads the configuration values from the disk into memory. If the file does not exist, Read() will create the file with default configuration values.
        /// </summary>
        public void Read()
        {
            bool configFileExists = File.Exists(_configFilePath);

            if (configFileExists)
            {
                var configDataAsString = System.IO.File.ReadAllText(_configFilePath);
                _dataOnDisk = JsonUtility.FromJson<T>(configDataAsString);
            }
            else
            {
                _dataOnDisk = new T();
            }

            Data = (T)_dataOnDisk.Clone();

            // if the config file does not currently exist, lets save it so it does
            if (!configFileExists)
            {
                Write();
            }
        }

        /// <summary>
        /// Is true if there are changes to the config that have not been written.
        /// </summary>
        public bool HasChanges
        {
            get
            {
                return (_dataOnDisk != Data);
            }
        }

        /// <summary>
        /// Writes the configuration data to the file that contains the information on disk.
        /// </summary>
        /// <param name="prettyPrint">Whether or not to format the data in a more human-readable manner.</param>
        public void Write(bool prettyPrint = true)
        {
            var configDataAsJSON = JsonUtility.ToJson(Data, prettyPrint);

            // If this is the first time we are saving the config, we need to create the directory
            // If the directory already exists this will do nothing
            FileInfo file = new(_configFilePath);
            file.Directory?.Create();

            File.WriteAllText(_configFilePath, configDataAsJSON);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}