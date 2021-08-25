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

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Epic.OnlineServices;
using Epic.OnlineServices.Reports;

using PlayEveryWare.EpicOnlineServices;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UIPlayerReportMenu : MonoBehaviour
    {
        public GameObject CrashReportUIParent;
        public Text PlayerName;
        public Dropdown CategoryList;
        public InputField Message;

        [Header("Controller")]
        public GameObject UIFirstSelected;

        private ProductUserId currentProdcutUserId;

        private EOSReportsManager ReportsManager;
        private EOSFriendsManager FriendsManager;

        private void Awake()
        {
            resetPopUp();
        }

        private void Start()
        {
            ReportsManager = EOSManager.Instance.GetOrCreateManager<EOSReportsManager>();
            FriendsManager = EOSManager.Instance.GetOrCreateManager<EOSFriendsManager>();
        }

        public void ReportButtonOnClick(ProductUserId userId, string playerName)
        {
            if(userId == null)
            {
                Debug.LogError("UIPlayerReportMenu (ReportButtonOnClick): ProductUserId is null!");
                return;
            }    

            PlayerName.text = playerName;
            currentProdcutUserId = userId;

            // Show PopUp
            CrashReportUIParent.gameObject.SetActive(true);

            // Controller
            if(UIFirstSelected.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(UIFirstSelected);
            }
        }

        public void SubmitReportButtonOnClick()
        {
            if(currentProdcutUserId == null || !currentProdcutUserId.IsValid())
            {
                Debug.LogError("UIPlayerReportMenu (ReportButtonOnClick): ProductUserId is not valid!");
                return;
            }

            string categoryStr = CategoryList.options[CategoryList.value].text;

            PlayerReportsCategory category = PlayerReportsCategory.Invalid;
            var categoryParsed = Enum.Parse(typeof(PlayerReportsCategory), categoryStr);
            if(categoryParsed != null)
            {
                category = (PlayerReportsCategory)categoryParsed;
            }

            if (ReportsManager != null)
            {
                ReportsManager.SendPlayerBehaviorReport(currentProdcutUserId, category, Message.text);
                resetPopUp();
            }
        }

        public void CancelButtonOnClick()
        {
            resetPopUp();
        }

        private void resetPopUp()
        {
            PlayerName.text = string.Empty;
            CategoryList.value = 0;
            Message.text = string.Empty;

            currentProdcutUserId = null;
            CrashReportUIParent.gameObject.SetActive(false);
        }
    }
}
