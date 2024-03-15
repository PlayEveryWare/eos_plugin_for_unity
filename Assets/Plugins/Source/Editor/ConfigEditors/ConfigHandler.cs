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
    using System.Threading.Tasks;

    /// <summary>
    /// Used to represent a "handle" to the config data, thus delegating all the IO work here.
    /// </summary>
    /// <typeparam name="T">The Config being represented</typeparam>
    public class ConfigHandler<T> where T : EpicOnlineServices.Config, new()
    {
        /// <summary>
        /// The Config data as it exists in memory.
        /// </summary>
        public T Data;

        /// <summary>
        /// Reads the configuration values from the disk into memory. If the file does not exist, Read() will create the file with default configuration values.
        /// </summary>
        public async Task Read()
        {
            Data = await EpicOnlineServices.Config.CreateAsync<T>();
        }

        /// <summary>
        /// Writes the configuration data to the file that contains the information on disk.
        /// </summary>
        /// <param name="prettyPrint">Whether or not to format the data in a more human-readable manner.</param>
        public async void Write(bool prettyPrint = true)
        {
            await Data.WriteAsync(prettyPrint);
        }
    }
}