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

using Epic.OnlineServices.Sessions;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UISessionEntry : MonoBehaviour
    {
        public Text NameTxt;
        public Text StatusTxt;
        public Text PlayersTxt;
        public Text LevelTxt;
        public Text JIPTxt;
        public Text PermissionTxt;
        public Text InvitesTxt;

        public Button JoinButton;
        [HideInInspector]
        public SessionDetails JoinSessionDetails = null;

        public Button StartButton;
        public Button EndButton;
        public Button ModifyButton;
        public Button LeaveButton;

        // Callbacks
        public Action<string> StartOnClick;
        public Action<string> EndOnClick;
        public Action<string> ModifyOnClick;
        public Action<string> LeaveOnClick;

        public Action<SessionDetails> JoinOnClick;

        public void OnlyEnableSearchResultButtons()
        {
            NameTxt.transform.gameObject.SetActive(false);

            JoinButton.transform.gameObject.SetActive(true);
            StartButton.interactable = true;

            StartButton.interactable = false;
            EndButton.interactable = false;
            ModifyButton.interactable = false;
            LeaveButton.interactable = false;
        }

        public void EnableButtonsBySessionState(bool updating, OnlineSessionState state)
        {
            StartButton.interactable = !updating && (state == OnlineSessionState.Pending || state == OnlineSessionState.Ended);
            EndButton.interactable = !updating && state == OnlineSessionState.InProgress;
        }

        public void JoinOnClickHandler()
        {
            if (JoinOnClick != null)
            {
                JoinOnClick(JoinSessionDetails);
            }
            else
            {
                Debug.LogError("UISessionEntry: JoinOnClick is not defined!");
            }
        }

        public void StartOnClickHandler()
        {
            if (StartOnClick != null)
            {
                //StartButton.interactable = false;
                //EndButton.interactable = true;
                StartOnClick(NameTxt.text);
            }
            else
            {
                Debug.LogError("UISessionEntry: StartOnClick is not defined!");
            }
        }

        public void EndOnClickHandler()
        {
            if (EndOnClick != null)
            {
                EndOnClick(NameTxt.text);
                //StartButton.interactable = true;
                //EndButton.interactable = false;
            }
            else
            {
                Debug.LogError("UISessionEntry: EndOnClick is not defined!");
            }
        }

        public void ModOnClickHandler()
        {
            if (ModifyOnClick != null)
            {
                ModifyOnClick(NameTxt.text);
            }
            else
            {
                Debug.LogError("UISessionEntry: ModOnClick is not defined!");
            }
        }

        public void LeaveOnClickHandler()
        {
            if (LeaveOnClick != null)
            {
                LeaveOnClick(NameTxt.text);
            }
            else
            {
                Debug.LogError("UISessionEntry: LeaveOnClick is not defined!");
            }
        }
    }
}