using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Epic.OnlineServices;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UIFriendEntry : MonoBehaviour
    {
        public Text Status;
        public Text DisplayName;
        public Button InviteButton;
        public Button AddButton;
        public Button ChatButton;

        private EpicAccountId accountId;

        // Callbacks
        public Action<EpicAccountId> AddFriendOnClick;
        public Action<EpicAccountId> InviteFriendsOnClick;
        public Action<EpicAccountId> ChatOnClick;

        private void Awake()
        {
            InviteButton.gameObject.SetActive(false);
            AddButton.gameObject.SetActive(false);
            ChatButton.gameObject.SetActive(false);
            accountId = null;
        }

        public void SetEpicAccountId(EpicAccountId accountId)
        {
            this.accountId = accountId;
        }

        public void EnableInviteButton(bool enable = true)
        {
            InviteButton.gameObject.SetActive(enable);
        }

        [Obsolete("EnableAddButton is obsolete.")]
        public void EnableAddButton(bool enable = true)
        {
            AddButton.gameObject.SetActive(enable);
        }

        public void EnableChatButton(bool enable = true)
        {
            ChatButton.gameObject.SetActive(enable);
        }

        public void AddFriendOnClickHandler()
        {
            if (accountId == null)
            {
                Debug.LogError("UIFriendEntry (AddFriendOnClickHandler): accountId is not set!");
                return;
            }

            if (AddFriendOnClick != null)
            {
                AddFriendOnClick(accountId);
            }
            else
            {
                Debug.LogError("UIFriendEntry (AddFriendOnClickHandler): AddFriendOnClick is not defined!");
            }
        }

        public void InviteFriendOnClickHandler()
        {
            if (accountId == null)
            {
                Debug.LogError("UIFriendEntry (InviteFriendOnClickHandler): accountId is not set!");
                return;
            }

            if (InviteFriendsOnClick != null)
            {
                InviteFriendsOnClick(accountId);
            }
            else
            {
                Debug.LogError("UIFriendEntry (InviteFriendOnClickHandler): InviteFriendsOnClick is not defined!");
            }
        }

        public void ChatButtonOnClickHandler()
        {
            if (accountId == null)
            {
                Debug.LogError("UIFriendEntry (ChatButtonOnClickHandler): accountId is not set!");
                return;
            }

            if (ChatOnClick != null)
            {
                ChatOnClick(accountId);
            }
            else
            {
                Debug.LogError("UIFriendEntry (ChatButtonOnClickHandler): ChatOnClick is not defined!");
            }
        }
    }
}