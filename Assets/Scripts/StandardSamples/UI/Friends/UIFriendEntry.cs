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
    
    using UnityEngine;
    using UnityEngine.UI;

    using Epic.OnlineServices;

    public class UIFriendEntry : MonoBehaviour
    {
        public Text Status;
        public Text DisplayName;
        public Button FriendInteractButton;
        public Text FriendInteractButtonText;

        private FriendData friendData;

        // Callbacks
        public Action<FriendData> FriendInteractOnClick;
        public Action<ProductUserId, String> ReportOnClick;

        public void SetFriendData(FriendData data)
        {
            friendData = data;
            DisplayName.text = data.Name;
        }

        public void EnableFriendButton(bool enable = true)
        {
            FriendInteractButton.gameObject.SetActive(enable);
        }

        public void EnableFriendButtonInteraction(bool enable = true)
        {
            FriendInteractButton.interactable = enable;
        }

        public void SetFriendbuttonText(string text)
        {
            FriendInteractButtonText.text = text;
        }

        public void FriendInteractOnClickHandler()
        {
            if (friendData == null)
            {
                Debug.LogError("UIFriendEntry (FriendInteractOnClickHandler): friendData is not set!");
                return;
            }

            if (FriendInteractOnClick != null)
            {
                FriendInteractOnClick(friendData);
            }
            else
            {
                Debug.LogError("UIFriendEntry (FriendInteractOnClickHandler): FriendInteractOnClickHandler is not defined!");
            }
        }
    }
}