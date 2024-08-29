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

    using UnityEngine;

    using Epic.OnlineServices;
    using Epic.OnlineServices.CustomInvites;

    public class CustomInviteData
    {
        public Utf8String InviteId;
        public Utf8String Payload;
        public ProductUserId Sender;
        public ProductUserId Recipient;
    }

    /// <summary>
    /// Class <c>EOSCustomInvitesManager</c> is a simplified wrapper for EOS [Custom Invites Interface](https://dev.epicgames.com/docs/services/en-US/GameServices/CustomInvites/index.html).
    /// </summary>
    public class EOSCustomInvitesManager : IEOSSubManager
    {
        private CustomInvitesInterface CustomInvitesHandle;

        private List<Action<CustomInviteData>> CustomInviteReceivedCallbacks;
        private List<Action<CustomInviteData>> CustomInviteAcceptedCallbacks;
        private List<Action<CustomInviteData>> CustomInviteRejectedCallbacks;

        private Dictionary<Utf8String, CustomInviteData> PendingInvites;

        public EOSCustomInvitesManager()
        {
            CustomInviteReceivedCallbacks = new List<Action<CustomInviteData>>();
            CustomInviteAcceptedCallbacks = new List<Action<CustomInviteData>>();
            CustomInviteRejectedCallbacks = new List<Action<CustomInviteData>>();
            PendingInvites = new Dictionary<Utf8String, CustomInviteData>();

            CustomInvitesHandle = EOSManager.Instance.GetEOSPlatformInterface().GetCustomInvitesInterface();

            var receiveOptions = new AddNotifyCustomInviteReceivedOptions();
            CustomInvitesHandle.AddNotifyCustomInviteReceived(ref receiveOptions, null, OnCustomInviteReceived);

            var acceptOptions = new AddNotifyCustomInviteAcceptedOptions();
            CustomInvitesHandle.AddNotifyCustomInviteAccepted(ref acceptOptions, null, OnCustomInviteAccepted);

            var rejectOptions = new AddNotifyCustomInviteRejectedOptions();
            CustomInvitesHandle.AddNotifyCustomInviteRejected(ref rejectOptions, null, OnCustomInviteRejected);
        }

        private void FinalizeInvite(Utf8String InviteId)
        {
            if (!PendingInvites.TryGetValue(InviteId, out CustomInviteData inviteData))
            {
                Debug.LogErrorFormat("CustomInvites (FinalizeInvite): invite {0} not found", InviteId);
                return;
            }

            var options = new FinalizeInviteOptions()
            {
                CustomInviteId = inviteData.InviteId,
                TargetUserId = inviteData.Sender,
                LocalUserId = inviteData.Recipient,
                //this should pass results to the overlay somehow, but the expected values are not clearly documented
                ProcessingResult = Result.Success
            };

            var result = CustomInvitesHandle.FinalizeInvite(ref options);

            if (result == Result.Success)
            {
                Debug.LogFormat("CustomInvites (FinalizeInvite): finalized invite {0}", InviteId);
            }
            else
            {
                Debug.LogErrorFormat("CustomInvites (FinalizeInvite): failed to finalize invite {0} with result {1}", InviteId, result.ToString());
            }

            PendingInvites.Remove(inviteData.InviteId);
        }

        private void OnCustomInviteReceived(ref OnCustomInviteReceivedCallbackInfo data)
        {
            if (PendingInvites.ContainsKey(data.CustomInviteId))
            {
                Debug.Log($"CustomInvites (OnCustomInviteReceived): Invite {data.CustomInviteId} already pending");
            }
            else
            {
                Debug.Log($"CustomInvites (OnCustomInviteReceived): Received invite {data.CustomInviteId}");

                var inviteData = new CustomInviteData()
                {
                    InviteId = data.CustomInviteId,
                    Payload = data.Payload,
                    Sender = data.TargetUserId,
                    Recipient = data.LocalUserId
                };
                PendingInvites.Add(inviteData.InviteId, inviteData);

                foreach (var callback in CustomInviteReceivedCallbacks)
                {
                    callback?.Invoke(inviteData);
                }
            }
        }

        /// <summary>
        /// Use to access functionality of [EOS_CustomInvites_AddNotifyCustomInviteReceived](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/CustomInvites/EOS_CustomInvites_AddNotifyCustomInviteReceived/index.html)
        /// </summary>
        /// <param name="Callback">Callback to receive notification when custom invite is received</param>
        public void AddNotifyCustomInviteReceived(Action<CustomInviteData> Callback)
        {
            CustomInviteReceivedCallbacks.Add(Callback);
        }

        public void RemoveNotifyCustomInviteReceived(Action<CustomInviteData> Callback)
        {
            CustomInviteReceivedCallbacks.Remove(Callback);
        }

        private void OnCustomInviteAccepted(ref OnCustomInviteAcceptedCallbackInfo data)
        {
            Debug.Log("CustomInvites (OnCustomInviteAccepted)");
            if (!PendingInvites.TryGetValue(data.CustomInviteId, out CustomInviteData inviteData))
            {
                Debug.LogErrorFormat("CustomInvites (OnCustomInviteAccepted): invite {0} not found", data.CustomInviteId);
            }
            else
            {
                foreach (var callback in CustomInviteAcceptedCallbacks)
                {
                    callback?.Invoke(inviteData);
                }
            }
            FinalizeInvite(data.CustomInviteId);
        }

        /// <summary>
        /// Use to access functionality of [EOS_CustomInvites_AddNotifyCustomInviteAccepted](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/CustomInvites/EOS_CustomInvites_AddNotifyCustomInviteAccepted/index.html)
        /// </summary>
        /// <param name="Callback">Callback to receive notification when custom invite is accepted</param>
        public void AddNotifyCustomInviteAccepted(Action<CustomInviteData> Callback)
        {
            CustomInviteAcceptedCallbacks.Add(Callback);
        }

        public void RemoveNotifyCustomInviteAccepted(Action<CustomInviteData> Callback)
        {
            CustomInviteAcceptedCallbacks.Remove(Callback);
        }

        private void OnCustomInviteRejected(ref CustomInviteRejectedCallbackInfo data)
        {
            Debug.Log("CustomInvites (OnCustomInviteRejected)");
            if (!PendingInvites.TryGetValue(data.CustomInviteId, out CustomInviteData inviteData))
            {
                Debug.LogErrorFormat("CustomInvites (OnCustomInviteRejected): invite {0} not found", data.CustomInviteId);
            }
            else
            {
                foreach (var callback in CustomInviteRejectedCallbacks)
                {
                    callback?.Invoke(inviteData);
                }
            }
            FinalizeInvite(data.CustomInviteId);
        }

        /// <summary>
        /// Use to access functionality of <c>EOS_CustomInvites_AddNotifyCustomInviteRejected</c>
        /// </summary>
        /// <param name="Callback">Callback to receive notification when custom invite is rejected</param>
        public void AddNotifyCustomInviteRejected(Action<CustomInviteData> Callback)
        {
            CustomInviteRejectedCallbacks.Add(Callback);
        }

        public void RemoveNotifyCustomInviteRejected(Action<CustomInviteData> Callback)
        {
            CustomInviteRejectedCallbacks.Remove(Callback);
        }

        /// <summary>
        /// Accept invite and trigger behavior of <c>AddNotifyCustomInviteAccepted</c>
        /// </summary>
        /// <param name="InviteId"><c>Utf8String</c> ID of invite to accept</param>
        public void AcceptInvite(Utf8String InviteId)
        {
            if (!PendingInvites.TryGetValue(InviteId, out CustomInviteData inviteData))
            {
                Debug.LogErrorFormat("CustomInvites (AcceptInvite): invite {0} not found", InviteId);
                return;
            }

            var callbackData = new OnCustomInviteAcceptedCallbackInfo()
            {
                CustomInviteId = inviteData.InviteId,
                Payload = inviteData.Payload,
                TargetUserId = inviteData.Sender,
                LocalUserId = inviteData.Recipient
            };
            //use same code path as accepting invite through overlay
            OnCustomInviteAccepted(ref callbackData);
        }

        /// <summary>
        /// Reject invite and trigger behavior of <c>AddNotifyCustomInviteRejected</c>
        /// </summary>
        /// <param name="InviteId"><c>Utf8String</c> ID of invite to reject</param>
        public void RejectInvite(Utf8String InviteId)
        {
            if (!PendingInvites.TryGetValue(InviteId, out CustomInviteData inviteData))
            {
                Debug.LogErrorFormat("CustomInvites (RejectInvite): invite {0} not found", InviteId);
                return;
            }

            var callbackData = new CustomInviteRejectedCallbackInfo()
            {
                CustomInviteId = inviteData.InviteId,
                Payload = inviteData.Payload,
                TargetUserId = inviteData.Sender,
                LocalUserId = inviteData.Recipient
            };
            //use same code path as rejecting invite through overlay
            OnCustomInviteRejected(ref callbackData);
        }

        /// <summary>
        /// Clear invite and trigger <c>FinalizeInvite</c> behavior without accepting or rejecting invite
        /// </summary>
        /// <param name="InviteId"><c>Utf8String</c> ID of invite to dismiss</param>
        public void DismissInvite(Utf8String InviteId)
        {
            if (!PendingInvites.TryGetValue(InviteId, out CustomInviteData inviteData))
            {
                Debug.LogErrorFormat("CustomInvites (DismissInvite): invite {0} not found", InviteId);
                return;
            }

            FinalizeInvite(inviteData.InviteId);
        }

        /// <summary>
        /// Wrapper for functionality of [EOS_CustomInvites_SetCustomInvite](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/CustomInvites/EOS_CustomInvites_SetCustomInvite/index.html)
        /// </summary>
        /// <param name="InvitePayload"><c>Utf8String</c> payload to send with custom invites</param>
        public void SetPayload(Utf8String InvitePayload)
        {
            var userId = EOSManager.Instance?.GetProductUserId();
            if (userId?.IsValid() != true)
            {
                Debug.LogError("CustomInvites (SetPayload): invalid product user ID");
                return;
            }

            var options = new SetCustomInviteOptions()
            {
                LocalUserId = userId,
                Payload = InvitePayload
            };

            var result = CustomInvitesHandle.SetCustomInvite(ref options);

            if (result == Result.Success)
            {
                if (string.IsNullOrEmpty(InvitePayload))
                {
                    Debug.Log("CustomInvites (SetPayload): custom payload cleared");
                }
                else
                {
                    Debug.LogFormat("CustomInvites (SetPayload): custom payload set to \"{0}\"", InvitePayload);
                }
            }
            else
            {
                Debug.LogError("CustomInvites (SetPayload): failed to set custom invite payload");
            }
        }

        public void ClearPayload()
        {
            //payload can be cleared by setting an empty string
            SetPayload(string.Empty);
        }

        /// <summary>
        /// Wrapper for functionality of [EOS_CustomInvites_SendCustomInvite](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/CustomInvites/EOS_CustomInvites_SendCustomInvite/index.html)
        /// </summary>
        /// <param name="RecipientId"><c>ProductUserId</c> of invite recipient</param>
        public void SendInvite(ProductUserId RecipientId, OnSendCustomInviteCallback Callback = null)
        {
            bool invalidUser = false;
            if (!RecipientId.IsValid())
            {
                Debug.LogError("CustomInvites (SetPayload): invalid recipient ID");
                invalidUser = true;
            }

            var userId = EOSManager.Instance.GetProductUserId();
            if (!userId.IsValid())
            {
                Debug.LogError("CustomInvites (SetPayload): invalid sender ID");
                invalidUser = true;
            }

            if (invalidUser)
            {
                if (Callback != null)
                {
                    var callbackData = new SendCustomInviteCallbackInfo()
                    {
                        ResultCode = Result.InvalidProductUserID,
                        LocalUserId = userId,
                        TargetUserIds = new ProductUserId[] { RecipientId }
                    };
                    Callback.Invoke(ref callbackData);
                }
                return;
            }

            var options = new SendCustomInviteOptions()
            {
                LocalUserId = userId,
                TargetUserIds = new ProductUserId[] { RecipientId }
            };

            CustomInvitesHandle.SendCustomInvite(ref options, Callback, OnSendCustomInvite);
        }

        private void OnSendCustomInvite(ref SendCustomInviteCallbackInfo data)
        {
            if (data.ResultCode == Result.Success)
            {
                Debug.Log("CustomInvites (OnSendCustomInvite): custom invite sent");
            }
            else
            {
                Debug.Log("CustomInvites (OnSendCustomInvite): failed to send custom invite");
            }

            var callback = data.ClientData as OnSendCustomInviteCallback;
            callback?.Invoke(ref data);
        }
    }
}