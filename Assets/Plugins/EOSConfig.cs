using System;
using UnityEngine;

namespace PlayEveryWare.EpicOnlineServices
{
    [Serializable]
    public class EOSConfig
    {
        public string productName;
        public string productVersion;

        public string productID;
        public string sandboxID;
        public string deploymentID;

        public string clientSecret;
        public string clientID;
        public string encryptionKey;

        public EOSConfig Clone()
        {
            return (EOSConfig)this.MemberwiseClone();
        }
    }
}
