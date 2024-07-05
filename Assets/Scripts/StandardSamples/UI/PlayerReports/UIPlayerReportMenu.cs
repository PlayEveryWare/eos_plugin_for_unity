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
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Epic.OnlineServices;
using Epic.OnlineServices.Reports;

using PlayEveryWare.EpicOnlineServices;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UIPlayerReportMenu : UIFriendInteractionSource, ISampleSceneUI
    {
        [Header("Reports")]
        public GameObject CrashReportUIParent;
        public Text PlayerName;
        public Dropdown CategoryList;
        public InputField Message;

        [Header("Sanctions")]
        public GameObject SanctionsListContentParent;
        public GameObject UISanctionsEntryPrefab;

        [Header("Controller")]
        public GameObject UIFirstSelected;

        private ProductUserId currentProdcutUserId;

        private EOSReportsManager ReportsManager;
        private EOSFriendsManager FriendsManager;

        private void Awake()
        {
            ResetPopUp();
        }

        private void Start()
        {
            ReportsManager = EOSManager.Instance.GetOrCreateManager<EOSReportsManager>();
            FriendsManager = EOSManager.Instance.GetOrCreateManager<EOSFriendsManager>();
        }

        private void OnDestroy()
        {
            EOSManager.Instance.RemoveManager<EOSReportsManager>();
            EOSManager.Instance.RemoveManager<EOSFriendsManager>();
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

            // Start Search for Sanctions
            EOSManager.Instance.GetOrCreateManager<EOSReportsManager>().QueryActivePlayerSanctions(userId, QueryActivePlayerSanctionsCompleted);

            // Show PopUp
            CrashReportUIParent.gameObject.SetActive(true);

            // Controller
            if(UIFirstSelected.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(UIFirstSelected);
            }
        }

        public void PlayerSanctionsRefreshOnClick()
        {
            // Start Search for Sanctions
            ReportsManager.QueryActivePlayerSanctions(currentProdcutUserId, QueryActivePlayerSanctionsCompleted);
        }

        private void QueryActivePlayerSanctionsCompleted(Result result)
        {
            if(result != Result.Success)
            {
                Debug.LogErrorFormat("UIPlayerReportMenu (QueryActivePlayerSanctionsCompleted): result == {0}", result);
                return;
            }

            // Destroy current UI member list
            foreach (Transform child in SanctionsListContentParent.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            // Update Sanctions List UI
            if (ReportsManager.GetCachedPlayerSanctions(out Dictionary<ProductUserId, List<Sanction>> sanctionLookup))
            {
                if(!sanctionLookup.ContainsKey(currentProdcutUserId))
                {
                    // No Sanctions for current user

                    GameObject sanctionUIObj = Instantiate(UISanctionsEntryPrefab, SanctionsListContentParent.transform);
                    UISanctionEntry uiEntry = sanctionUIObj.GetComponent<UISanctionEntry>();

                    uiEntry.TimePlaced.text = string.Empty;
                    uiEntry.Action.text = "No Sanctions Found.";

                    return;
                }

                List<Sanction> sanctionList = sanctionLookup[currentProdcutUserId];

                foreach (Sanction s in sanctionList)
                {
                    GameObject sanctionUIObj = Instantiate(UISanctionsEntryPrefab, SanctionsListContentParent.transform);
                    UISanctionEntry uiEntry = sanctionUIObj.GetComponent<UISanctionEntry>();

                    uiEntry.TimePlaced.text = string.Format("Added on {0: M/d/yyyy HH:mm}", s.TimePlaced);
                    uiEntry.Action.text = s.Action;
                }
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
                ResetPopUp();
            }
        }

        public void CancelButtonOnClick()
        {
            ResetPopUp();
        }

        private void ResetPopUp()
        {
            PlayerName.text = string.Empty;
            CategoryList.value = 0;
            Message.text = string.Empty;

            currentProdcutUserId = null;
            CrashReportUIParent.gameObject.SetActive(false);
        }

        public override FriendInteractionState GetFriendInteractionState(FriendData friendData)
        {
            return FriendInteractionState.Enabled;
        }

        public override string GetFriendInteractButtonText()
        {
            return "Report";
        }

        public override void OnFriendInteractButtonClicked(FriendData friendData)
        {
            ReportButtonOnClick(friendData.UserProductUserId, friendData.Name);
        }

        public void ShowMenu()
        {
            ResetPopUp();
        }

        public void HideMenu()
        {
            ResetPopUp();
        }
    }
}
