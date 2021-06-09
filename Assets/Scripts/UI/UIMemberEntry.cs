using System;
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
        public Button KickButton;
        public Button Promotebutton;

        // Callbacks
        public Action<ProductUserId> KickOnClick;
        public Action<ProductUserId> PromoteOnClick;

        // Metadata
        [HideInInspector]
        public string MemberName = string.Empty;

        [HideInInspector]
        public ProductUserId ProductUserId;

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

            IsOwnerText.text = isMemberEntryOwner.ToString();

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