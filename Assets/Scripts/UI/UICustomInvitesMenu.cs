﻿/*
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

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UICustomInvitesMenu : UIInviteSource, ISampleSceneUI
    {
        [Header("Custom Invites UI")]
        public Transform PendingInvitesParent;
        public UICustomInviteEntry PendingInviteEntryPrefab;
        public Text InviteLogText;

        private EOSCustomInvitesManager CustomInvitesManager;
        private EOSFriendsManager FriendsManager;
        private List<UICustomInviteEntry> PendingInviteEntries;
        private bool PayloadSet = false;

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
        }

        public void HideMenu()
        {
            gameObject.SetActive(false);
        }

        public void ShowMenu()
        {
            gameObject.SetActive(true);
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

        public void OnPayloadEndEdit(string payloadText)
        {
            CustomInvitesManager.SetPayload(payloadText);
            PayloadSet = true;
        }

        public override bool IsInviteActive()
        {
            return PayloadSet;
        }

        public override void OnInviteButtonClicked(EpicAccountId UserId)
        {
            FriendsManager.GetCachedFriends(out Dictionary<EpicAccountId, FriendData> friends);

            if (friends.TryGetValue(UserId, out FriendData friend))
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