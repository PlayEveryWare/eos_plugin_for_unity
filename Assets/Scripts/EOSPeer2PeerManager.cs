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
    /// Enum <c>messageType</c> is used to indicate the type of message in <c>messageData</c>.
    /// </summary>
    public enum messageType
    {
        textMessage = 0,
        coordinatesMessage = 1
    };
    /// <summary>
    /// Enum <c>messageData</c> is used to store a message <c>messageData</c>.
    /// </summary>
    public struct messageData
    {
        public messageType type;
        public string dataArray;
    };

    public struct messageTypeText
    {
        public int length;
        public string message;
    }
    public struct messageTypeCoordinates
    {
        public int x;
        public int y;
    }

    /// <summary>
    /// Class <c>EOSPeer2PeerManager</c> is a simplified wrapper for EOS [P2P Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/P2P/index.html).
    /// </summary>

    
    public class EOSPeer2PeerManager : IEOSSubManager
    {
        private P2PInterface P2PHandle;

        private ulong ConnectionNotificationId;
        private Dictionary<ProductUserId, ChatWithFriendData> ChatDataCache;
        private bool ChatDataCacheDirty;

        public UIPeer2PeerParticleManager ParticleManager;
        public Transform parent;


        public EOSPeer2PeerManager()
        {
            P2PHandle = EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface();

            ChatDataCache = new Dictionary<ProductUserId, ChatWithFriendData>();
            ChatDataCacheDirty = true;
        }

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

        
        public messageTypeText messageToTextMessage(messageData data)
        {
            messageTypeText message;
            message.message = data.dataArray.Substring(1);
            message.length = message.message.Length;
            return message;
        }


        //   message type
        //     |  x coord
        //     |  ______
        //    \|/|      |               
        //    |1|1|4|5|6|,|1|2|3|4|
        //                  |_______|
        //                   y coord
        //       
        //   
        public messageTypeCoordinates MessageToCoordinates(messageData data)
        {
            messageTypeCoordinates coords = new messageTypeCoordinates();
            string substring = data.dataArray.Substring(1);

            string[] temp = substring.Split(',');
            coords.x = Int32.Parse(temp[0]);
            coords.y = Int32.Parse(temp[1]);

            return coords;
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

                messageTypeText textMessage = messageToTextMessage(message);
                if (string.IsNullOrEmpty(textMessage.message))
                {
                    Debug.LogError("EOS P2PNAT SendMessage: bad input data message is empty.");
                    return;
                }

                // Update Cache
                ChatEntry chatEntry = new ChatEntry()
                {
                    isOwnEntry = true,
                    Message = textMessage.message
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

                SendPacketOptions options = CreateSendPacketOptions(socketId, friendId, message.dataArray);

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

                
                // Send Message
                SocketId socketId = new SocketId()
                {
                    SocketName = "CHAT"
                };

                SendPacketOptions options = CreateSendPacketOptions(socketId, friendId, message.dataArray);

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

        public SendPacketOptions CreateSendPacketOptions(SocketId socketId, ProductUserId friendId, string data)
        {
            SendPacketOptions options = new SendPacketOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RemoteUserId = friendId,
                SocketId = socketId,
                AllowDelayedDelivery = true,
                Channel = 0,
                Reliability = PacketReliability.ReliableOrdered,
                Data = new ArraySegment<byte>(Encoding.UTF8.GetBytes(data))
            };

            return options;
        }

        public messageData deserializeMessage(byte[] data)
        {
            messageData message = new messageData();
            string tempString = data[0].ToString();
            int tempInt = Convert.ToInt32(tempString);
            message.type = (messageType) tempInt - 48; //convert from ascii decimal to the integer value of the ascii character
            message.dataArray = System.Text.Encoding.UTF8.GetString(data);
            return message;
        }
        public ProductUserId HandleReceivedMessages()
        {
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

                messageData message = deserializeMessage(data);

                if (message.type == messageType.textMessage)
                {
                    messageTypeText textMessage = messageToTextMessage(message);

                    ChatEntry newMessage = new ChatEntry()
                    {
                        isOwnEntry = false,
                        Message = textMessage.message
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
                else if (message.type == messageType.coordinatesMessage)
                {
                    messageTypeCoordinates coords = MessageToCoordinates(message);
                    Debug.Log("EOS P2PNAT HandleReceivedMessages:  Mouse position Recieved at " + coords.x + ", " + coords.y);

                    ParticleManager.SpawnParticles(coords.x, coords.y, parent);

                    return peerId;
                }



                else
                {
                    Debug.LogErrorFormat("EOS P2PNAT HandleReceivedMessages: error while reading data, code: {0}", result);
                    Debug.Log("Message Type was: " + message.type);
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