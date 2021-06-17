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
    public struct ChatEntry
    {
        public bool isOwnEntry;
        public string Message;
    }
    public struct ChatWithFriendData
    {
        public Queue<ChatEntry> ChatLines;

        public ProductUserId FriendId;

        public ChatWithFriendData(ProductUserId FriendId)
        {
            this.FriendId = FriendId;
            ChatLines = new Queue<ChatEntry>();
        }
    }

    public class EOSPeer2PeerManager : IEOSSubManager
    {
        private P2PInterface P2PHandle;

        private ulong ConnectionNotificationId;
        private Dictionary<ProductUserId, ChatWithFriendData> ChatDataCache;
        private bool ChatDataCacheDirty;

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
            P2PHandle.QueryNATType(new QueryNATTypeOptions(), null, OnRefreshNATTypeFinished);
        }

        public NATType GetNATType()
        {
            Result result = P2PHandle.GetNATType(new GetNATTypeOptions(), out NATType natType);

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

        private void OnRefreshNATTypeFinished(OnQueryNATTypeCompleteInfo data)
        {
            if (data == null)
            {
                Debug.LogError("P2P (OnRefreshNATTypeFinished): data is null");
                return;
            }

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("P2p (OnRefreshNATTypeFinished): RefreshNATType error: {0}", data.ResultCode);
                return;
            }

            Debug.Log("P2p (OnRefreshNATTypeFinished): RefreshNATType Completed");
        }

        public void SendMessage(ProductUserId friendId, string message)
        {
            if (!friendId.IsValid() || string.IsNullOrEmpty(message))
            {
                Debug.LogError("EOS P2PNAT SendMessage: bad input data (account id is wrong or message is empty).");
                return;
            }

            // Update Cache
            ChatEntry chatEntry = new ChatEntry()
            {
                isOwnEntry = true,
                Message = message
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
                Data = Encoding.UTF8.GetBytes(message)
            };

            Result result = P2PHandle.SendPacket(options);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("EOS P2PNAT SendMessage: error while sending data, code: {0}", result);
                return;
            }

            Debug.Log("EOS P2PNAT SendMessage: Message successfully sent to user.");
        }

        public ProductUserId HandleReceivedMessages()
        {
            ReceivePacketOptions options = new ReceivePacketOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                MaxDataSizeBytes = 4096,
                RequestedChannel = null
            };

            Result result = P2PHandle.ReceivePacket(options, out ProductUserId peerId, out SocketId socketId, out byte outChannel, out byte[] data);

            if (result == Result.NotFound)
            {
                // no packets
                return null;
            }
            else if (result == Result.Success)
            {
                //Do something with chat output
                Debug.LogFormat("Message received: peerId={0}, socketId={1}, data={2}", peerId, socketId, Encoding.UTF8.GetString(data));

                if(!peerId.IsValid())
                {
                    Debug.LogErrorFormat("EOS P2PNAT HandleReceivedMessages: ProductUserId peerId is not valid!");
                    return null;
                }

                ChatEntry newMessage = new ChatEntry()
                {
                    isOwnEntry = false,
                    Message = System.Text.Encoding.UTF8.GetString(data)
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
            else
            {
                Debug.LogErrorFormat("EOS P2PNAT HandleReceivedMessages: error while reading data, code: {0}", result);
                return null;
            }
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

                ConnectionNotificationId = P2PHandle.AddNotifyPeerConnectionRequest(options, null, OnIncomingConnectionRequest);
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

        private void OnIncomingConnectionRequest(OnIncomingConnectionRequestInfo data)
        {
            if (data == null)
            {
                Debug.LogError("P2P (OnIncomingConnectionRequest): data is null");
                return;
            }

            if (!data.SocketId.SocketName.Equals("CHAT"))
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

            Result result = P2PHandle.AcceptConnection(options);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("P2p (OnIncomingConnectionRequest): error while accepting connection, code: {0}", result);
            }
        }
    }
}