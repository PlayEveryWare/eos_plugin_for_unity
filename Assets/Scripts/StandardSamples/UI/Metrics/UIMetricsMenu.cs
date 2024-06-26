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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using UnityEngine;
using Epic.OnlineServices;
using Epic.OnlineServices.Metrics;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UIMetricsMenu : MonoBehaviour, ISampleSceneUI
    {
        public UIConsoleInputField DisplayNameVal;
        public Dropdown ControllerTypeDropdown;
        public UIConsoleInputField ServerIpVal;
        public Text ErrorMessageTxt;
        public UIConsoleInputField SessionIdVal;
        public Button BeginBtn;
        public Button EndBtn;

        private Regex ipv4Validator;
        private Regex ipv6Validator;
        private EOSMetricsManager MetricsManager;
        private EOSUserInfoManager UserInfoManager;

        private void Awake()
        {
            ipv4Validator = new Regex("^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");
            ipv6Validator = new Regex("(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))");
        }

        private void Start()
        {
            MetricsManager = EOSManager.Instance.GetOrCreateManager<EOSMetricsManager>();
            UserInfoManager = EOSManager.Instance.GetOrCreateManager<EOSUserInfoManager>();

            var controlOptions = new List<Dropdown.OptionData>();
            foreach (string controlType in Enum.GetNames(typeof(UserControllerType)))
            {
                controlOptions.Add(new Dropdown.OptionData(controlType));
            }
            ControllerTypeDropdown.options = controlOptions;
            ControllerTypeDropdown.value = 0;

            ErrorMessageTxt.gameObject.SetActive(false);
            UpdateButtons();
        }

        private void OnDestroy()
        {
            EOSManager.Instance.RemoveManager<EOSMetricsManager>();
        }

        private bool ValidateIp(ref string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = null;
                return true;
            }
            else
            {
                ip = ip.Trim();
                return ipv4Validator.IsMatch(ip) || ipv6Validator.IsMatch(ip);
            }
        }

        private void LogError(string error)
        {
            ErrorMessageTxt.text = error;
            Debug.LogError("UIMetricsMenu: "+error);
        }

        private bool CheckError()
        {
            if (ErrorMessageTxt.text != string.Empty)
            {
                ErrorMessageTxt.gameObject.SetActive(true);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void UpdateButtons()
        {
            bool sessionActive = MetricsManager.IsSessionActive();
            BeginBtn.interactable = !sessionActive;
            EndBtn.interactable = sessionActive;
        }

        public void BeginSessionOnClick()
        {
            ErrorMessageTxt.text = string.Empty;

            string displayName = DisplayNameVal.InputField.text.Trim();
            if (string.IsNullOrWhiteSpace(displayName))
            {
                LogError("Display name required");
            }

            UserControllerType controllerType = (UserControllerType)Enum.Parse(typeof(UserControllerType), ControllerTypeDropdown.options[ControllerTypeDropdown.value].text, true);

            string serverIp = ServerIpVal.InputField.text;
            if(!ValidateIp(ref serverIp))
            {
                LogError("Server IP must be blank or a valid IP address (IPv4 or IPv6)");
            }

            string sessionId = SessionIdVal.InputField.text.Trim();
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                sessionId = null;
            }

            if (!CheckError())
            {
                MetricsManager.BeginSession(displayName, controllerType, serverIp, sessionId);
            }

            UpdateButtons();
        }

        public void EndSessionOnClick()
        {
            MetricsManager.EndSession();
            UpdateButtons();
        }

        public void HideMenu()
        {
            gameObject.SetActive(false);
            UpdateButtons();
        }

        public void ShowMenu()
        {
            gameObject.SetActive(true);
            var localUserInfo = UserInfoManager.GetLocalUserInfo();
            if (localUserInfo.UserId?.IsValid() == true)
            {
                DisplayNameVal.InputField.text = localUserInfo.DisplayName;
            }
            else
            {
                DisplayNameVal.InputField.text = string.Empty;
                var localUserId = EOSManager.Instance.GetLocalUserId();
                if (localUserId?.IsValid() == true)
                {
                    UserInfoManager.QueryUserInfoById(localUserId, OnLocalUserQueryFinished);
                }
            }
            DisplayNameVal.InputField.text = localUserInfo.UserId?.IsValid() == true ? localUserInfo.DisplayName.ToString() : string.Empty;
            UpdateButtons();
        }

        private void OnLocalUserQueryFinished(EpicAccountId UserId, Result QueryResult)
        {
            if (QueryResult == Result.Success && DisplayNameVal.InputField.text == string.Empty)
            {
                var localUserInfo = UserInfoManager.GetLocalUserInfo();
                if (localUserInfo.UserId?.IsValid() == true)
                {
                    DisplayNameVal.InputField.text = localUserInfo.DisplayName;
                }
            }
        }
    }
}