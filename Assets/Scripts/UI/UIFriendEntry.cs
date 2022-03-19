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
        public Button ReportButton;

        private EpicAccountId accountId;
        private ProductUserId productUserId;

        // Callbacks
        public Action<EpicAccountId> AddFriendOnClick;
        public Action<EpicAccountId> InviteFriendsOnClick;
        public Action<EpicAccountId> ChatOnClick;
        public Action<ProductUserId, String> ReportOnClick;

        private void Awake()
        {
            InviteButton.gameObject.SetActive(false);
            AddButton.gameObject.SetActive(false);
            ChatButton.gameObject.SetActive(false);
            ReportButton.gameObject.SetActive(false);
            accountId = null;
            productUserId = null;
        }

        public void SetEpicAccount(EpicAccountId accountId, ProductUserId productUserId)
        {
            this.accountId = accountId;
            this.productUserId = productUserId;
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

        public void EnableReportButton(bool enable = true)
        {
            ReportButton.gameObject.SetActive(enable);
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

        public void ReportButtonOnClickHander()
        {
            if (accountId == null)
            {
                Debug.LogError("UIFriendEntry (ReportButtonOnClickHander): accountId is not set!");
                return;
            }

            if(productUserId == null)
            {
                Debug.LogWarning("UIFriendEntry (ReportButtonOnClickHander): productUserId is null (still querying), try again later.");
                return;
            }

            if (ReportOnClick != null)
            {
                ReportOnClick(productUserId, DisplayName.text);
            }
            else
            {
                Debug.LogError("UIFriendEntry (ReportButtonOnClickHander): ReportOnClick is not defined!");
            }
        }
    }
}