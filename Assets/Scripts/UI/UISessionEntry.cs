using System;
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
        public Text PresenceTxt;
        public Text JIPTxt;
        public Text PublicTxt;
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

        private void Start()
        {
            // Prefab is destroyed and state is not maintained between updates
            //StartButton.interactable = true;
            //EndButton.interactable = false;
        }

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