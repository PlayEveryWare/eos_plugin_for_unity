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

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UIChatEntry : MonoBehaviour
    {
        public GameObject RemoteChatPanel;
        public Text RemoteUser;
        public Text RemoteMessage;

        public GameObject LocalChatPanel;
        public Text LocalUser;
        public Text LocalMessage;

        public void SetLocalEntry(string DisplayName, string Message)
        {
            LocalUser.text = DisplayName;
            LocalUser.gameObject.SetActive(true);

            LocalChatPanel.SetActive(true);

            LocalMessage.text = Message;
            LocalMessage.gameObject.SetActive(true);
        }

        public void SetRemoteEntry(string DisplayName, string Message)
        {
            RemoteUser.text = DisplayName;
            RemoteUser.gameObject.SetActive(true);

            RemoteChatPanel.SetActive(true);

            RemoteMessage.text = Message;
            RemoteMessage.gameObject.SetActive(true);
        }
    }
}