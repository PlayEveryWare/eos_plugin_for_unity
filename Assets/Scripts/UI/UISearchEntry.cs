using System;
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