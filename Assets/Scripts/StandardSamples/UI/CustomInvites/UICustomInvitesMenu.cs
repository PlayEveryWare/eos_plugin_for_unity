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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Epic.OnlineServices;
using Epic.OnlineServices.CustomInvites;
using Epic.OnlineServices.Presence;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UICustomInvitesMenu : UIFriendInteractionSource, ISampleSceneUI
    {
        [Header("Custom Invites UI")]
        public UIConsoleInputField PayloadInputField;
        public Transform PendingInvitesParent;
        public UICustomInviteEntry PendingInviteEntryPrefab;
        public Text InviteLogText;

        private EOSCustomInvitesManager CustomInvitesManager;
        private EOSFriendsManager FriendsManager;
        private List<UICustomInviteEntry> PendingInviteEntries;
        private bool PayloadSet = false;
        private bool UIDirty = false;

        private void Awake()
        {
            PendingInviteEntries = new List<UICustomInviteEntry>();
            FriendsManager = EOSManager.Instance.GetOrCreateManager<EOSFriendsManager>();
            CustomInvitesManager = EOSManager.Instance.GetOrCreateManager<EOSCustomInvitesManager>();
            CustomInvitesManager.AddNotifyCustomInviteReceived(OnInviteReceived);
            CustomInvitesManager.AddNotifyCustomInviteAccepted(OnInviteAccepted);
            CustomInvitesManager.AddNotifyCustomInviteRejected(OnInviteRejected);
            HideMenu();
        }

        private void OnDestroy()
        {
            CustomInvitesManager?.RemoveNotifyCustomInviteReceived(OnInviteReceived);
            CustomInvitesManager?.RemoveNotifyCustomInviteAccepted(OnInviteAccepted);
            CustomInvitesManager?.RemoveNotifyCustomInviteRejected(OnInviteRejected);

            EOSManager.Instance.RemoveManager<EOSFriendsManager>();
            EOSManager.Instance.RemoveManager<EOSCustomInvitesManager>();
        }

        public void HideMenu()
        {
            gameObject.SetActive(false);
            if (EOSManager.Instance.GetProductUserId()?.IsValid() == true)
            {
                CustomInvitesManager?.ClearPayload();
            }
        }

        public void ShowMenu()
        {
            PayloadInputField.InputField.text = string.Empty;
            CustomInvitesManager.ClearPayload();
            gameObject.SetActive(true);

            var presenceInterface = EOSManager.Instance.GetEOSPresenceInterface();
            var presenceModificationOptions = new CreatePresenceModificationOptions();
            presenceModificationOptions.LocalUserId = EOSManager.Instance.GetLocalUserId();

            Result result = presenceInterface.CreatePresenceModification(ref presenceModificationOptions, out PresenceModification presenceModificationHandle);

            if (result == Result.Success)
            {

                //mark user as online
                var presenceModificationSetStatusOptions = new PresenceModificationSetStatusOptions();
                presenceModificationSetStatusOptions.Status = Status.Online;
                presenceModificationHandle.SetStatus(ref presenceModificationSetStatusOptions);

                var presenceModificationSetJoinOptions = new PresenceModificationSetJoinInfoOptions();

                presenceModificationSetJoinOptions.JoinInfo = "Custom Invite";
                presenceModificationHandle.SetJoinInfo(ref presenceModificationSetJoinOptions);

                // actually update all the status changes
                var setPresenceOptions = new Epic.OnlineServices.Presence.SetPresenceOptions();
                setPresenceOptions.LocalUserId = EOSManager.Instance.GetLocalUserId();
                setPresenceOptions.PresenceModificationHandle = presenceModificationHandle;
                presenceInterface.SetPresence(ref setPresenceOptions, null, (ref SetPresenceCallbackInfo data) => { });
                presenceModificationHandle.Release();
            }
        }

        private string GetSenderName(CustomInviteData InviteData)
        {
            var senderEpicId = FriendsManager.GetAccountMapping(InviteData.Sender);
            var senderName = FriendsManager.GetDisplayName(senderEpicId);
            if (string.IsNullOrEmpty(senderName))
            {
                return InviteData.Sender.ToString();
            }
            else
            {
                return senderName;
            }
        }

        private void OnInviteReceived(CustomInviteData InviteData)
        {
            var newEntry = Instantiate(PendingInviteEntryPrefab, PendingInvitesParent);
            var senderName = GetSenderName(InviteData);
            newEntry.SetInviteData(InviteData, senderName);
            newEntry.SetCallbacks(CustomInvitesManager.AcceptInvite, CustomInvitesManager.RejectInvite);
            PendingInviteEntries.Add(newEntry);
        }

        private void RemoveInviteEntry(CustomInviteData InviteData)
        {
            var entryToRemove = PendingInviteEntries.Find((UICustomInviteEntry entry) => entry.InviteId == InviteData.InviteId);
            if (entryToRemove != null)
            {
                PendingInviteEntries.Remove(entryToRemove);
                Destroy(entryToRemove.gameObject);
            }
        }

        private void OnInviteAccepted(CustomInviteData InviteData)
        {
            var senderName = GetSenderName(InviteData);
            InviteLogText.text += string.Format("Invite from {0} accepted\n", senderName);
            RemoveInviteEntry(InviteData);
        }

        private void OnInviteRejected(CustomInviteData InviteData)
        {
            var senderName = GetSenderName(InviteData);
            InviteLogText.text += string.Format("Invite from {0} rejected\n", senderName);
            RemoveInviteEntry(InviteData);
        }

        public void SetPayloadOnClick()
        {
            string payloadText = PayloadInputField.InputField.text.Trim();
            if (string.IsNullOrEmpty(payloadText))
            {
                CustomInvitesManager.ClearPayload();
                PayloadSet = false;
            }
            else
            {
                CustomInvitesManager.SetPayload(payloadText);
                PayloadSet = true;
            }
            UIDirty = true;
        }

        public override FriendInteractionState GetFriendInteractionState(FriendData friendData)
        {
            return PayloadSet && friendData.IsFriend() && friendData.IsOnline() ? FriendInteractionState.Enabled : FriendInteractionState.Disabled;
        }

        public override string GetFriendInteractButtonText()
        {
            return "Invite";
        }

        public override bool IsDirty()
        {
            return UIDirty;
        }

        public override void ResetDirtyFlag()
        {
            UIDirty = false;
        }

        public override void OnFriendInteractButtonClicked(FriendData friendData)
        {
            FriendsManager.GetCachedFriends(out Dictionary<EpicAccountId, FriendData> friends);

            if (friends.TryGetValue(friendData.UserId, out FriendData friend))
            {
                if (friend.UserProductUserId == null || !friend.UserProductUserId.IsValid())
                {
                    Debug.LogError("UICustomInvitesMenu (OnInviteButtonClicked): UserProductUserId is not valid!");
                }
                else
                {
                    CustomInvitesManager.SendInvite(friend.UserProductUserId);
                }
            }
            else
            {
                Debug.LogError("UICustomInvitesMenu (OnInviteButtonClicked): Friend not found in cached data.");
            }
        }
    }
}