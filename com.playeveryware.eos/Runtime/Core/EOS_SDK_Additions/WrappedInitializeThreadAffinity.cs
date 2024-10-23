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

namespace PlayEveryWare.EpicOnlineServices
{
    using Epic.OnlineServices.Platform;

    public struct WrappedInitializeThreadAffinity
    {
        private InitializeThreadAffinity _value;

        /// <summary>
        /// Any thread related to network management that is not IO.
        /// </summary>
        [ConfigField("Network", ConfigFieldType.Ulong, "Any thread related to network management that is not IO.")]
        public ulong NetworkWork
        {
            get
            {
                return _value.NetworkWork;
            }
            set
            {
                _value.NetworkWork = value;
            }
        }

        /// <summary>
        /// Any thread that will interact with a storage device.
        /// </summary>
        [ConfigField("Storage IO", ConfigFieldType.Ulong, "Any thread that will interact with a storage device.")]
        public ulong StorageIo
        {
            get
            {
                return _value.StorageIo;
            }
            set
            {
                _value.StorageIo = value;
            }
        }

        /// <summary>
        /// Any thread that will generate web socket IO.
        /// </summary>
        [ConfigField("Web Socket IO", ConfigFieldType.Ulong, "Any thread that will generate web socket IO.")]
        public ulong WebSocketIo
        {
            get
            {
                return _value.WebSocketIo;
            }
            set
            {
                _value.WebSocketIo = value;
            }
        }

        /// <summary>   
        /// Any thread that will generate IO related to P2P traffic and management.
        /// </summary>
        [ConfigField("P2P IO", ConfigFieldType.Ulong, "Any thread that will generate IO related to P2P traffic and management.")]
        public ulong P2PIo
        {
            get
            {
                return _value.P2PIo;
            }
            set
            {
                _value.P2PIo = value;
            }
        }

        /// <summary>
        /// Any thread that will generate http request IO.
        /// </summary>
        [ConfigField("HTTP Request IO", ConfigFieldType.Ulong, "Any thread that will generate http request IO.")]
        public ulong HttpRequestIo
        {
            get
            {
                return _value.HttpRequestIo;
            }
            set
            {
                _value.HttpRequestIo = value;
            }
        }

        /// <summary>
        /// Any thread that will generate IO related to RTC traffic and management.
        /// </summary>
        [ConfigField("RTC IO", ConfigFieldType.Ulong, "Any thread that will generate IO related to RTC traffic and management.")]
        public ulong RTCIo
        {
            get
            {
                return _value.RTCIo;
            }
            set
            {
                _value.RTCIo = value;
            }
        }

        /// <summary>
        /// Main thread of the external overlay
        /// </summary>
        [ConfigField("Embedded Overlay Main Thread", ConfigFieldType.Ulong, "Main thread of the external overlay.")]
        public ulong EmbeddedOverlayMainThread
        {
            get
            {
                return _value.EmbeddedOverlayMainThread;
            }
            set
            {
                _value.EmbeddedOverlayMainThread = value;
            }
        }

        /// <summary>
        /// Worker threads of the external overlay
        /// </summary>
        [ConfigField("Embedded Overlay Worker Threads", ConfigFieldType.Ulong, "Worker threads of the external overlay.")]
        public ulong EmbeddedOverlayWorkerThreads
        {
            get
            {
                return _value.EmbeddedOverlayWorkerThreads;
            }
            set
            {
                _value.EmbeddedOverlayWorkerThreads = value;
            }
        }
    }
}