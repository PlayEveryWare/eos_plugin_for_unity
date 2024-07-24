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
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

#define EOS_RUNTIME_NEW_CONFIG_SYSTEM

namespace PlayEveryWare.EpicOnlineServices
{
    using System;
    using System.Collections.Generic;
#if EOS_RUNTIME_NEW_CONFIG_SYSTEM
    public class RuntimeConfig : Config
    {
        /// <summary>
        /// Product Name defined in the
        /// [Development Portal](https://dev.epicgames.com/portal/)
        /// </summary>
        public string productName;

        /// <summary>
        /// Version of Product.
        /// </summary>
        public Version productVersion;

        /// <summary>
        /// Product Id defined in the
        /// [Development Portal](https://dev.epicgames.com/portal/)
        /// </summary>
        public Guid productID;

        /// <summary>
        /// Deployment Id defined in the
        /// [Development Portal](https://dev.epicgames.com/portal/)
        /// </summary>
        public (Guid sandbox, Guid deployment) sandboxDeployment;

        /// <summary>
        /// Sandbox deployments pairs used to override Deployment ID when
        /// a given Sandbox ID is used.
        ///
        /// The key is the sandbox ID, because sandboxes can have multiple
        /// deployments. The value (the array of Guid values) represents the
        /// deployments available for that sandbox.
        /// </summary>
        public Dictionary<Guid, Guid[]> sandboxDeploymentOverrides;

        /// <summary>
        /// Client Secret defined in the
        /// [Development Portal](https://dev.epicgames.com/portal/)
        /// </summary>
        public string clientSecret;

        /// <summary>
        /// Client Id defined in the
        /// [Development Portal](https://dev.epicgames.com/portal/)
        /// </summary>
        public string clientID;
        
        /// <summary>
        /// Encryption Key&lt; used by default to decode files previously
        /// encoded and stored in EOS.
        /// </summary>
        public string encryptionKey;

        /// <summary>
        /// Flags; used to initialize the EOS platform.
        /// </summary>
        public List<PlatformOptionsFalgs> platformOptionsFlags;

        /// <summary>
        /// Flags; used to set user auth when logging in.
        /// </summary>
        public List<string> authScopeOptionsFlags;

        /// <summary>
        /// Tick Budget; used to define the maximum amount of execution time the
        /// EOS SDK can use each frame.
        /// </summary>
        public uint tickBudgetInMilliseconds;

        /// <summary>
        /// Network Work Affinity; specifies thread affinity for network
        /// management that is not IO.
        /// </summary>
        public string ThreadAffinity_networkWork;

        /// <summary>
        /// Storage IO Affinity; specifies affinity for threads that will
        /// interact with a storage device.
        /// </summary>
        public string ThreadAffinity_storageIO;

        /// <summary>
        /// Web Socket IO Affinity; specifies affinity for threads that generate
        /// web socket IO.
        /// </summary>
        public string ThreadAffinity_webSocketIO;

        /// <summary>
        /// P2P IO Affinity; specifies affinity for any thread that will
        /// generate IO related to P2P traffic and management.
        /// </summary>
        public string ThreadAffinity_P2PIO;

        /// <summary>
        /// HTTP Request IO Affinity; specifies affinity for any thread that
        /// will generate http request IO.
        /// </summary>
        public string ThreadAffinity_HTTPRequestIO;

        /// <summary>
        /// RTC IO Affinity&lt;/c&gt; specifies affinity for any thread that
        /// will generate IO related to RTC traffic and management.
        /// </summary>
        public string ThreadAffinity_RTCIO;

        /// <summary>
        /// Always Send Input to Overlay &lt;/c&gt;If true, the plugin will
        /// always send input to the overlay from the C# side to native, and
        /// handle showing the overlay. This doesn't always mean input makes
        /// it to the EOS SDK.
        /// </summary>
        public bool alwaysSendInputToOverlay;

        /// <summary>
        /// Initial Button Delay; Stored as a string so it can be 'empty'
        /// </summary>
        public string initialButtonDelayForOverlay;

        /// <summary>
        /// Repeat button delay for overlay; Stored as a string so it can be
        /// 'empty'.
        /// </summary>
        public string repeatButtonDelayForOverlay;

        /// <summary>
        /// HACK: send force send input without delay&lt;/c&gt;If true, the
        /// native plugin will always send input received directly to the SDK.
        /// If set to false, the plugin will attempt to delay the input to
        /// mitigate CPU spikes caused by spamming the SDK.
        /// </summary>
        public bool hackForceSendInputDirectlyToSDK;

        /// <summary>
        /// Set to 'true' if the application is a dedicated game server.
        /// </summary>
        public bool isServer;

        public RuntimeConfig(string filename) : base(filename)
        {
         
        }
    }
#endif
}