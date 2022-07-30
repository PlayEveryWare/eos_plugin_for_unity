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

using PlayEveryWare.EpicOnlineServices;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UIMemberEntry : MonoBehaviour
    {
        public Text MemberNameText;
        public Text IsOwnerText;
        public Text IsTalkingText;
        public Button MuteButton;
        public Button KickButton;
        public Button Promotebutton;

        // Callbacks
        public Action<ProductUserId> MuteOnClick;
        public Action<ProductUserId> KickOnClick;
        public Action<ProductUserId> PromoteOnClick;

        // Metadata
        [HideInInspector]
        public string MemberName = string.Empty;

        [HideInInspector]
        public ProductUserId ProductUserId;

        public void UpdateMemberData(LobbyMember Member)
        {
            string displayName = Member.DisplayName;
            if (!string.IsNullOrEmpty(displayName))
            {
                MemberName = displayName;
            }
            else
            {
                Result result = Member.ProductId.ToString(out Utf8String outBuff);
                if (result == Result.Success)
                {
                    MemberName = outBuff;
                }
                else
                {
                    MemberName = "Error: " + result;
                }
            }

            ProductUserId = Member.ProductId;
        }

        public void UpdateUI()
        {
            MemberNameText.text = MemberName;

            bool isPlayerLobbyOwner = false;
            EOSLobbyManager lobbyManager = EOSManager.Instance.GetOrCreateManager<EOSLobbyManager>();
            if (lobbyManager.GetCurrentLobby().IsValid() && lobbyManager.GetCurrentLobby().LobbyOwner == EOSManager.Instance.GetProductUserId())
            {
                isPlayerLobbyOwner = true;
            }

            bool isMemberEntryOwner = false;
            if (lobbyManager.GetCurrentLobby().IsValid() && lobbyManager.GetCurrentLobby().LobbyOwner == ProductUserId)
            {
                isMemberEntryOwner = true;
            }

            //IsOwnerText.text = isMemberEntryOwner.ToString();

            if (isPlayerLobbyOwner && !isMemberEntryOwner)
            {
                KickButton.enabled = true;
                Promotebutton.enabled = true;

                KickButton.gameObject.SetActive(true);
                Promotebutton.gameObject.SetActive(true);
            }
            else
            {
                KickButton.enabled = false;
                Promotebutton.enabled = false;

                KickButton.gameObject.SetActive(false);
                Promotebutton.gameObject.SetActive(false);
            }

            if(lobbyManager.GetCurrentLobby().RTCRoomEnabled)
            {
                MuteButton.enabled = true;
                MuteButton.gameObject.SetActive(true);

                foreach(LobbyMember member in lobbyManager.GetCurrentLobby().Members)
                {
                    if(member.ProductId == ProductUserId)
                    {
                        // Update Talking state
                        if (member.RTCState.IsTalking)
                        {
                            IsTalkingText.text = "Talking";
                        }
                        else if (member.RTCState.IsAudioOutputDisabled || member.RTCState.IsLocalMuted)
                        {
                            IsTalkingText.text = "Muted";
                        }
                        else
                        {
                            IsTalkingText.text = "Silent";
                        }
                        break;
                    }
                }
            }
            else
            {
                MuteButton.enabled = false;
                MuteButton.gameObject.SetActive(false);

                IsTalkingText.text = string.Empty;
            }
        }

        public void MemberEntryMuteButtonOnClick()
        {
            if (MuteOnClick != null)
            {
                MuteOnClick(ProductUserId);
            }
            else
            {
                Debug.LogError("MemberEntryMuteButtonOnClick: MuteOnClick action is null!");
            }
        }

        public void MemberEntryKickButtonOnClick()
        {
            if (KickOnClick != null)
            {
                KickOnClick(ProductUserId);
            }
            else
            {
                Debug.LogError("MemberEntryKickButtonOnClick: KickOnClick action is null!");
            }
        }

        public void MemberEntryPromoteButtonOnClick()
        {
            if (PromoteOnClick != null)
            {
                PromoteOnClick(ProductUserId);
            }
            else
            {
                Debug.LogError("MemberEntryPromoteButtonOnClick: PromoteOnClick action is null!");
            }
        }
    }
}