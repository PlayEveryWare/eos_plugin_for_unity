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
using Epic.OnlineServices.Lobby;

using PlayEveryWare.EpicOnlineServices;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UISearchEntry : MonoBehaviour
    {
        public Text OwnerNameTxt;
        public Text MembersTxt;
        public Text LevelTxt;

        public Button JoinButton;

        [HideInInspector]
        public Action<Lobby, LobbyDetails> JoinButtonOnClick;

        // Metadata
        [HideInInspector]
        public string OwnerName = string.Empty;

        [HideInInspector]
        public int Members = 0;
        [HideInInspector]
        public int MaxMembers = 0;

        [HideInInspector]
        public string Level = string.Empty;

        [HideInInspector]
        public Lobby LobbyRef;
        [HideInInspector]
        public LobbyDetails LobbyDetailsRef;

        public void UpdateUI()
        {
            OwnerNameTxt.text = OwnerName;
            MembersTxt.text = string.Format("{0}/{1}", Members, MaxMembers);
            LevelTxt.text = Level;

            JoinButton.enabled = true;
        }

        public void SearchEntryJoinButtonOnClick()
        {
            if (JoinButtonOnClick != null)
            {
                JoinButtonOnClick(LobbyRef, LobbyDetailsRef);
            }
            else
            {
                Debug.LogError("UISearchEntry: JoinButtonOnClick action is null!");
            }
        }
    }
}