using System.Collections;
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