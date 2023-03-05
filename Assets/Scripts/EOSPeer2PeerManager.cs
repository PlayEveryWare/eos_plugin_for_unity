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

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Epic.OnlineServices;
using Epic.OnlineServices.P2P;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    /// <summary>
    /// Struct <c>ChatEntry</c> is used to store cached chat data in <c>UIPeer2PeerMenu</c>.
    /// </summary>

    public struct ChatEntry
    {
        /// <value>True if message was from local user</value>
        public bool isOwnEntry;

        /// <value> Cache for message entry </value>
        public string Message;
    }

    /// <summary>
    /// Struct <c>ChatWithFriendData</c> is used to store cached friend chat data in <c>UIPeer2PeerMenu</c>.
    /// </summary>

    public struct ChatWithFriendData
    {
        /// <value> Queue of cached <c>ChatEntry</c> objects </value>
        public Queue<ChatEntry> ChatLines;

        /// <value> <c>FriendId</c> of remote friend </value>
        public ProductUserId FriendId;

        /// <summary> Constructor for creating a new local cache of chat entries.</summary>
        /// <param name="FriendId"><c>ProductUserId</c> of remote friend</param>
        public ChatWithFriendData(ProductUserId FriendId)
        {
            this.FriendId = FriendId;
            ChatLines = new Queue<ChatEntry>();
        }
    }

    /// <summary>
    /// Class <c>EOSPeer2PeerManager</c> is a simplified wrapper for EOS [P2P Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/P2P/index.html).
    /// </summary>
    public enum messageType
    {
        textMessage,
        coordinatesMessage
    };
    public struct messageData
    {
        public messageType type;
        public string textData;

        public float xPos;
        public float yPos;
    };

    public class EOSPeer2PeerManager : IEOSSubManager
    {
        private P2PInterface P2PHandle;

        private ulong ConnectionNotificationId;
        private Dictionary<ProductUserId, ChatWithFriendData> ChatDataCache;
        private bool ChatDataCacheDirty;

        public UIPeer2PeerParticleController ParticleController;
        public Transform parent;

#if UNITY_EDITOR
        void OnPlayModeChanged(UnityEditor.PlayModeStateChange modeChange)
        {
            if (modeChange == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                //prevent attempts to call native EOS code while exiting play mode, which crashes the editor
                P2PHandle = null;
            }
        }
#endif

        public EOSPeer2PeerManager()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChanged;
#endif

            P2PHandle = EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface();

            ChatDataCache = new Dictionary<ProductUserId, ChatWithFriendData>();
            ChatDataCacheDirty = true;
        }

#if UNITY_EDITOR
        ~EOSPeer2PeerManager()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }
#endif

        public bool GetChatDataCache(out Dictionary<ProductUserId, ChatWithFriendData> ChatDataCache)
        {
            ChatDataCache = this.ChatDataCache;
            return ChatDataCacheDirty;
        }

        private void RefreshNATType()
        {
            var options = new QueryNATTypeOptions();
            P2PHandle.QueryNATType(ref options, null, OnRefreshNATTypeFinished);
        }

        public NATType GetNATType()
        {
            var options = new GetNATTypeOptions();
            Result result = P2PHandle.GetNATType(ref options, out NATType natType);

            if (result == Result.NotFound)
            {
                return NATType.Unknown;
            }

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("EOS P2PNAT GetNatType: error while retrieving NAT Type: {0}", result);
                return NATType.Unknown;
            }

            return natType;
        }

        public void OnLoggedIn()
        {
            RefreshNATType();

            SubscribeToConnectionRequest();
        }

        public void OnLoggedOut()
        {
            UnsubscribeFromConnectionRequests();
        }

        private void OnRefreshNATTypeFinished(ref OnQueryNATTypeCompleteInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("P2P (OnRefreshNATTypeFinished): data is null");
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("P2p (OnRefreshNATTypeFinished): RefreshNATType error: {0}", data.ResultCode);
                return;
            }

            Debug.Log("P2p (OnRefreshNATTypeFinished): RefreshNATType Completed");
        }

        public void SendMessage(ProductUserId friendId, messageData message)
        {
            if (!friendId.IsValid())
            {
                Debug.LogError("EOS P2PNAT SendMessage: bad input data: account id is wrong.");
                return;
            }
            if (message.type == messageType.textMessage)
            {
                if (string.IsNullOrEmpty(message.textData))
                {
                    Debug.LogError("EOS P2PNAT SendMessage: bad input data message is empty.");
                    return;
                }

                // Update Cache
                ChatEntry chatEntry = new ChatEntry()
                {
                    isOwnEntry = true,
                    Message = message.textData
                };

                if (ChatDataCache.TryGetValue(friendId, out ChatWithFriendData chatData))
                {
                    chatData.ChatLines.Enqueue(chatEntry);
                    ChatDataCacheDirty = true;
                }
                else
                {
                    ChatWithFriendData newChatData = new ChatWithFriendData(friendId);
                    newChatData.ChatLines.Enqueue(chatEntry);

                    ChatDataCache.Add(friendId, newChatData);
                    ChatDataCacheDirty = true;
                }

                // Send Message
                SocketId socketId = new SocketId()
                {
                    SocketName = "CHAT"
                };

                SendPacketOptions options = new SendPacketOptions()
                {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    RemoteUserId = friendId,
                    SocketId = socketId,
                    AllowDelayedDelivery = true,
                    Channel = 0,
                    Reliability = PacketReliability.ReliableOrdered,
                    Data = new ArraySegment<byte>(Encoding.UTF8.GetBytes("t" + message.textData))
                };

                Result result = P2PHandle.SendPacket(ref options);

                if (result != Result.Success)
                {
                    Debug.LogErrorFormat("EOS P2PNAT SendMessage: error while sending data, code: {0}", result);
                    return;
                }

                Debug.Log("EOS P2PNAT SendMessage: Message successfully sent to user.");
            }

            else if (message.type == messageType.coordinatesMessage)
            {

                string rawData = ("m" + message.xPos.ToString() + "," + message.yPos.ToString());
                // Send Message
                SocketId socketId = new SocketId()
                {
                    SocketName = "CHAT"
                };

                SendPacketOptions options = new SendPacketOptions()
                {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    RemoteUserId = friendId,
                    SocketId = socketId,
                    AllowDelayedDelivery = true,
                    Channel = 0,
                    Reliability = PacketReliability.ReliableOrdered,
                    Data = new ArraySegment<byte>(Encoding.UTF8.GetBytes(rawData))
                };

                Result result = P2PHandle.SendPacket(ref options);

                if (result != Result.Success)
                {
                    Debug.LogErrorFormat("EOS P2PNAT SendMessage: error while sending data, code: {0}", result);
                    return;
                }
            }

            else
            {
                Debug.Log("EOS P2PNAT SendMessage: Message content was not valid.");
            }
        }

        public ProductUserId HandleReceivedMessages()
        {
            if (P2PHandle == null)
            {
                return null;
            }

            ReceivePacketOptions options = new ReceivePacketOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                MaxDataSizeBytes = 4096,
                RequestedChannel = null
            };

            var getNextReceivedPacketSizeOptions = new GetNextReceivedPacketSizeOptions
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RequestedChannel = null
            };
            P2PHandle.GetNextReceivedPacketSize(ref getNextReceivedPacketSizeOptions, out uint nextPacketSizeBytes);

            byte[] data = new byte[nextPacketSizeBytes];
            var dataSegment = new ArraySegment<byte>(data);
            Result result = P2PHandle.ReceivePacket(ref options, out ProductUserId peerId, out SocketId socketId, out byte outChannel, dataSegment, out uint bytesWritten);

            if (result == Result.NotFound)
            {
                // no packets
                return null;
            }
            else if (result == Result.Success)
            {
                //Do something with chat output
                Debug.LogFormat("Message received: peerId={0}, socketId={1}, data={2}", peerId, socketId, Encoding.UTF8.GetString(data));

                if (!peerId.IsValid())
                {
                    Debug.LogErrorFormat("EOS P2PNAT HandleReceivedMessages: ProductUserId peerId is not valid!");
                    return null;
                }

                string message = System.Text.Encoding.UTF8.GetString(data);

                if (message.StartsWith("t"))
                {
                    ChatEntry newMessage = new ChatEntry()
                    {
                        isOwnEntry = false,
                        Message = message.Substring(1)
                    };

                    if (ChatDataCache.TryGetValue(peerId, out ChatWithFriendData chatData))
                    {
                        // Update existing chat
                        chatData.ChatLines.Enqueue(newMessage);

                        ChatDataCacheDirty = true;
                        return peerId;
                    }
                    else
                    {
                        ChatWithFriendData newChat = new ChatWithFriendData(peerId);
                        newChat.ChatLines.Enqueue(newMessage);

                        // New Chat Request
                        ChatDataCache.Add(peerId, newChat);

                        return peerId;
                    }
                }
                else if (message.StartsWith("m"))
                {
                    message = message.Substring(1);

                    string[] coords = message.Split(',');
                    int xPos = Int32.Parse(coords[0]);
                    int yPos = Int32.Parse(coords[1]);
                    Debug.Log("EOS P2PNAT HandleReceivedMessages:  Mouse position Recieved at " + xPos + ", " + yPos);

                    ParticleController.SpawnParticles(xPos, yPos, parent);

                    return peerId;
                }



                else
                {
                    Debug.LogErrorFormat("EOS P2PNAT HandleReceivedMessages: error while reading data, code: {0}", result);
                    return null;
                }
            }
            return null;
        }

        private void SubscribeToConnectionRequest()
        {
            if (ConnectionNotificationId == 0)
            {
                SocketId socketId = new SocketId()
                {
                    SocketName = "CHAT"
                };

                AddNotifyPeerConnectionRequestOptions options = new AddNotifyPeerConnectionRequestOptions()
                {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    SocketId = socketId
                };

                ConnectionNotificationId = P2PHandle.AddNotifyPeerConnectionRequest(ref options, null, OnIncomingConnectionRequest);
                if (ConnectionNotificationId == 0)
                {
                    Debug.Log("EOS P2PNAT SubscribeToConnectionRequests: could not subscribe, bad notification id returned.");
                }
            }
        }

        private void UnsubscribeFromConnectionRequests()
        {
            P2PHandle.RemoveNotifyPeerConnectionRequest(ConnectionNotificationId);
            ConnectionNotificationId = 0;
        }

        private void OnIncomingConnectionRequest(ref OnIncomingConnectionRequestInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("P2P (OnIncomingConnectionRequest): data is null");
            //    return;
            //}

            if (!(bool)data.SocketId?.SocketName.Equals("CHAT"))
            {
                Debug.LogError("P2p (OnIncomingConnectionRequest): bad socket id");
                return;
            }

            SocketId socketId = new SocketId()
            {
                SocketName = "CHAT"
            };

            AcceptConnectionOptions options = new AcceptConnectionOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RemoteUserId = data.RemoteUserId,
                SocketId = socketId
            };

            Result result = P2PHandle.AcceptConnection(ref options);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("P2p (OnIncomingConnectionRequest): error while accepting connection, code: {0}", result);
            }
        }
    }
}