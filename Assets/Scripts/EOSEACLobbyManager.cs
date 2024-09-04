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

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    
    using UnityEngine;
    
    using Epic.OnlineServices;
    using Epic.OnlineServices.Logging;
    using Epic.OnlineServices.AntiCheatClient;
    using Epic.OnlineServices.AntiCheatCommon;

    /// <summary>
    /// Class <c>EOSEACLobbyManager</c> manages testing functionality for the <c>EOSAntiCheatClientManager</c> using <c>EOSLobbyManager</c> to manage peer communication.
    /// </summary>
    public class EOSEACLobbyManager : IEOSSubManager
    {
        private EOSLobbyManager LobbyManager;
        private EOSAntiCheatClientManager AntiCheatManager;

        private string CurrentLobbyId = string.Empty;
        private HashSet<ProductUserId> CurrentLobbyPeers;

        private Dictionary<ProductUserId, int> OutgoingMessageCounters;
        private Dictionary<ProductUserId, int> IncomingMessageCounters;


        public EOSEACLobbyManager()
        {
            EOSManager.Instance.SetLogLevel(LogCategory.AntiCheat, LogLevel.Verbose);

            LobbyManager = EOSManager.Instance.GetOrCreateManager<EOSLobbyManager>();
            AntiCheatManager = EOSManager.Instance.GetOrCreateManager<EOSAntiCheatClientManager>();

            CurrentLobbyPeers = new HashSet<ProductUserId>();

            OutgoingMessageCounters = new Dictionary<ProductUserId, int>();
            IncomingMessageCounters = new Dictionary<ProductUserId, int>();

            if (AntiCheatManager.IsAntiCheatAvailable())
            {
                LobbyManager.LobbyChanged += OnLobbyChanged;
                LobbyManager.AddNotifyLobbyUpdate(OnLobbyUpdated);
                LobbyManager.AddNotifyMemberUpdateReceived(OnMemberUpdated);

                AntiCheatManager.AddNotifyToMessageToPeer(OnMessageToPeer);
                AntiCheatManager.AddNotifyPeerActionRequired(OnPeerActionRequired);
                AntiCheatManager.AddNotifyClientIntegrityViolated(OnClientIntegrityViolated);
            }
        }

        /// <summary>
        /// Called when the integrity of the client has been violated according to EAC
        /// </summary>
        /// <param name="data"></param>
        private void OnClientIntegrityViolated(ref OnClientIntegrityViolatedCallbackInfo data)
        {
            Debug.LogError("EAC Client Integrity Violeted!");
        }

        /// <summary>
        /// Check if EAC functionality is availble
        /// </summary>
        /// <returns>False if EAC client functionality is not available i.e. game was launched without EAC bootstrapper</returns>
        public bool IsAntiCheatAvailable()
        {
            return AntiCheatManager.IsAntiCheatAvailable();
        }

        private void OnMessageToPeer(ref OnMessageToClientCallbackInfo data)
        {
            var targetUserId = AntiCheatManager.GetPeerId(data.ClientHandle);
            if (targetUserId == null || !targetUserId.IsValid())
            {
                Debug.LogError("EACLobbyTest (OnMessageToPeer): invalid target user id");
                return;
            }

            string userIdKey = targetUserId.ToString().ToUpper();

            OutgoingMessageCounters.TryGetValue(targetUserId, out int counter);
            counter++;

            //encode anti-cheat peer message as base64 string and store it as a member attribute
            //along with a counter to distinguish already processed messages
            byte[] dataArray = new byte[4*3 + data.MessageData.Array.Length];
            byte[] counterBytes = BitConverter.GetBytes(counter);
            Array.Copy(counterBytes, 0, dataArray, 0, 4);
            byte[] dataCountBytes = BitConverter.GetBytes(data.MessageData.Count);
            Array.Copy(dataCountBytes, 0, dataArray, 4, 4);
            byte[] offsetBytes = BitConverter.GetBytes(data.MessageData.Offset);
            Array.Copy(offsetBytes, 0, dataArray, 4*2, 4);
            Array.Copy(data.MessageData.Array, 0, dataArray, 4 * 3, data.MessageData.Array.Length);

            Debug.LogFormat("EACLobbyTest (OnMessageToPeer): sending message, count: {0}, offset : {1}", data.MessageData.Count, data.MessageData.Offset);

            string dataString = Convert.ToBase64String(dataArray);

            var dataAttrib = new LobbyAttribute()
            {
                Visibility = Epic.OnlineServices.Lobby.LobbyAttributeVisibility.Private,
                Key = userIdKey,
                ValueType = AttributeType.String,
                AsString = dataString
            };

            LobbyManager.SetMemberAttribute(dataAttrib);
        }

        private void OnMemberUpdated(string LobbyId, ProductUserId MemberId)
        {
            if (!EOSManager.Instance.GetProductUserId().IsValid())
            {
                return;
            }

            var currentLobby = LobbyManager.GetCurrentLobby();
            string localUserKey = EOSManager.Instance.GetProductUserId().ToString().ToUpper();

            var targetMember = currentLobby.Members.Find((LobbyMember member) => { return member.ProductId == MemberId; });
            var peerHandle = AntiCheatManager.GetPeerHandle(MemberId);

            if (targetMember != null && peerHandle != IntPtr.Zero)
            { 
                targetMember.MemberAttributes.TryGetValue(localUserKey, out LobbyAttribute msgAttrib);
                if (msgAttrib?.ValueType == AttributeType.String)
                {
                    //decode base64 string message and pass to anti-cheat
                    string msgString = msgAttrib.AsString;
                    byte[] dataArray = Convert.FromBase64String(msgString);
                    int msgCounter = BitConverter.ToInt32(dataArray, 0);
                    int prevCounter;
                    if (!IncomingMessageCounters.TryGetValue(MemberId, out prevCounter) || prevCounter != msgCounter)
                    {
                        string dataString = msgString.Substring(1);
                        int count = BitConverter.ToInt32(dataArray, 4);
                        int offset = BitConverter.ToInt32(dataArray, 4 * 2);
                        byte[] msgArray = new byte[dataArray.Length - 12];

                        Debug.LogFormat("EACLobbyTest (OnMemberUpdated): received message, count: {0}, offset : {1}, length: {2}", count, offset, msgArray.Length);
                        Array.Copy(dataArray, 4 * 3, msgArray, 0, msgArray.Length);
                        ArraySegment<byte> dataPacket = new ArraySegment<byte>(msgArray, offset, count);
                        AntiCheatManager.ReceiveMessageFromPeer(peerHandle, dataPacket);

                        IncomingMessageCounters[MemberId] = msgCounter;
                    }
                }
            }
        }

        private void OnPeerActionRequired(ref OnClientActionRequiredCallbackInfo data)
        {
            var currentLobby = LobbyManager.GetCurrentLobby();

            var peerUserId = AntiCheatManager.GetPeerId(data.ClientHandle);
            var localUserId = EOSManager.Instance.GetProductUserId();

            if (data.ClientAction == AntiCheatCommonClientAction.RemovePlayer)
            {
                if (currentLobby.IsOwner(localUserId))
                {
                    Debug.LogFormat("EACLobbyTest (OnPeerActionRequired): kicking user for cheating, id: {0}, reason: {1}", peerUserId.ToString(), data.ActionReasonDetailsString);
                    LobbyManager.KickMember(peerUserId, null);
                }
                else if (currentLobby.IsOwner(peerUserId))
                {
                    Debug.LogFormat("EACLobbyTest (OnPeerActionRequired): leaving lobby due to owner cheating, id: {0}, reason: {1}", peerUserId.ToString(), data.ActionReasonDetailsString);
                    LobbyManager.LeaveLobby(null);
                }
            }      
        }

        private void OnLobbyUpdated()
        {
            var currentLobby = LobbyManager.GetCurrentLobby();
            if (!currentLobby.IsValid())
            {
                return;
            }

            HashSet<ProductUserId> currentLobbyMembers = new HashSet<ProductUserId>();
            var localUserId = EOSManager.Instance.GetProductUserId();

            //register all new members as peers
            foreach (var member in currentLobby.Members)
            {
                if (member.ProductId == localUserId)
                {
                    continue;
                }

                currentLobbyMembers.Add(member.ProductId);
                if (!CurrentLobbyPeers.Contains(member.ProductId))
                {
                    if (AntiCheatManager.RegisterPeer(member.ProductId))
                    {
                        CurrentLobbyPeers.Add(member.ProductId);
                    }
                }
            }

            //unregister all peers that are no longer in lobby
            var removedPeers = CurrentLobbyPeers.Except(currentLobbyMembers);
            foreach (var removedPeer in removedPeers)
            {
                if (AntiCheatManager.UnregisterPeer(removedPeer))
                {
                    CurrentLobbyPeers.Remove(removedPeer);
                }
            }
        }

        private void OnLobbyChanged(object sender, EOSLobbyManager.LobbyChangeEventArgs args)
        {
            string previousLobbyId = CurrentLobbyId;
            var currentLobby = LobbyManager.GetCurrentLobby();
            if (currentLobby.IsValid())
            {
                CurrentLobbyId = currentLobby.Id;
            }
            else
            {
                CurrentLobbyId = string.Empty;
            }

            if (CurrentLobbyId != previousLobbyId)
            {
                if (AntiCheatManager.IsSessionActive())
                {
                    AntiCheatManager.EndSession();
                }

                //remove all peers from previous lobby
                foreach (var peer in CurrentLobbyPeers)
                {
                    if (AntiCheatManager.UnregisterPeer(peer))
                    {
                        CurrentLobbyPeers.Remove(peer);
                    }
                }

                OnLobbyUpdated();

                if (currentLobby.IsValid())
                {
                    //begin protencted session if lobby is tagged with anticheat flag
                    var anticheatAttrib = currentLobby.Attributes.Find((LobbyAttribute attrib) => { return attrib.Key == "ANTICHEAT"; });
                    if (anticheatAttrib != null && anticheatAttrib.AsBool == true)
                    {
                        AntiCheatManager.BeginSession();
                    }
                }
            }
        }
    }
}