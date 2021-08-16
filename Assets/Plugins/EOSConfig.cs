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

using System;
using UnityEngine;

namespace PlayEveryWare.EpicOnlineServices
{

    /// <summary>
    /// Represents the EOS Configuration used for initializing EOS SDK.
    /// </summary>
    [Serializable]
    public class EOSConfig
    {
        /// <value><c>Product Name</c> defined in the [Development Portal](https://dev.epicgames.com/portal/)</value>
        public string productName;

        /// <value>Version of Product</value>
        public string productVersion;

        /// <value><c>Product Id</c> defined in the [Development Portal](https://dev.epicgames.com/portal/)</value>
        public string productID;

        /// <value><c>Sandbox Id</c> defined in the [Development Portal](https://dev.epicgames.com/portal/)</value>
        public string sandboxID;

        /// <value><c>Deployment Id</c> defined in the [Development Portal](https://dev.epicgames.com/portal/)</value>
        public string deploymentID;

        /// <value><c>Client Secret</c> defined in the [Development Portal](https://dev.epicgames.com/portal/)</value>
        public string clientSecret;

        /// <value><c>Client Id</c> defined in the [Development Portal](https://dev.epicgames.com/portal/)</value>
        public string clientID;

        /// <value><c>Encryption Key</c> used by default to decode files previously encoded and stored in EOS</value>
        public string encryptionKey;

        /// <summary>
        /// Creates a shallow copy of the current <c>EOSConfig</c>
        /// </summary>
        /// <returns>Shallow copy of <c>EOSConfig</c></returns>
        public EOSConfig Clone()
        {
            return (EOSConfig)this.MemberwiseClone();
        }
    }
}
