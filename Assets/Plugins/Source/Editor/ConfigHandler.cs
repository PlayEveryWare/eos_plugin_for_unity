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

using System.IO;
using UnityEditor;
using UnityEngine;

namespace PlayEveryWare.EpicOnlineServices
{
    //-------------------------------------------------------------------------
    public class EOSConfigFile<T> where T : ICloneableGeneric<T>, IEmpty, new()
    {
        public string configFilenamePath;
        public T configDataOnDisk;
        public T currentEOSConfig;

        //-------------------------------------------------------------------------
        public EOSConfigFile(string aConfigFilenamePath)
        {
            configFilenamePath = aConfigFilenamePath;
        }

        //-------------------------------------------------------------------------
        public void LoadConfigFromDisk()
        {
            string eosFinalConfigPath = configFilenamePath;
            bool jsonConfigExists = File.Exists(eosFinalConfigPath);

            if (jsonConfigExists)
            {
                var configDataAsString = System.IO.File.ReadAllText(eosFinalConfigPath);
                configDataOnDisk = JsonUtility.FromJson<T>(configDataAsString);
            }
            else
            {
                configDataOnDisk = new T();
            }
            currentEOSConfig = configDataOnDisk.Clone();
        }

        //-------------------------------------------------------------------------
        public void SaveToJSONConfig(bool prettyPrint)
        {
            if (currentEOSConfig.IsEmpty())
            {
                if (EOSPluginEditorConfigEditor.IsAsset(configFilenamePath))
                {
                    AssetDatabase.DeleteAsset(configFilenamePath);
                }
                else
                {
                    File.Delete(configFilenamePath);
                }
            }
            else
            {
                var configDataAsJSON = JsonUtility.ToJson(currentEOSConfig, prettyPrint);
                string configFilenameParentPath = Path.GetDirectoryName(configFilenamePath);

                if (!Directory.Exists(configFilenameParentPath))
                {
                    Directory.CreateDirectory(configFilenameParentPath);
                }


                // If this is the first time we are saving the config, we need to create the directory
                // If the directory already exists this will do nothing
                System.IO.FileInfo file = new System.IO.FileInfo(configFilenamePath);
                file.Directory.Create();
                
                File.WriteAllText(configFilenamePath, configDataAsJSON);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
