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

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using UnityEngine;
    using UnityEngine.UI;

    using Epic.OnlineServices.Achievements;
    
    /// <summary>
    /// Unity UI sample that uses <c>AchievementManager</c> to demo features.  Can be used as a template or starting point for implementing Achievement features.
    /// </summary>
    public class UIAchievementsMenu : SampleMenu
    {
        [Header("Achievements UI")]
        public Button refreshDataButton;
        public Button loginIncreaseButton;
        public Toggle showDefinitionToggle;
        public Button unlockAchievementButton;
        public Text definitionsDescription;
        public ScrollRect scrollRect;
        public Transform achievementListContainer;
        public UIAchievementButton itemTemplate;
        public RawImage achievementUnlockedIcon;
        public RawImage achievementLockedIcon;

        private List<UIAchievementButton> achievementListItems = new();

        private bool displayDefinition = false;
        private int displayIndex = -1;

        class AchievementData
        {
            public DefinitionV2 Definition;
            public PlayerAchievement? PlayerData;
        }
        List<AchievementData> achievementDataList = new();

        protected override void Awake()
        {
            base.Awake();
            AchievementsService.Instance.Updated += OnAchievementDataUpdated;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            AchievementsService.Instance.Updated -= OnAchievementDataUpdated;
        }

        public async void IncrementLoginStat()
        {
            await StatsService.Instance.IngestStatAsync("login_count", 1);
        }

        //manually unlock achievement being displayed
        //TODO: refresh achievement data without having to log out
        public async void UnlockAchievement()
        {
            if (displayIndex < 0 || displayIndex > AchievementsService.GetAchievementsCount())
            {
                return;
            }

            var definition = AchievementsService.Instance.GetAchievementDefinitionAtIndex(displayIndex);

            try
            {
                await AchievementsService.Instance.UnlockAchievementAsync(definition.AchievementId);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        public async void OnRefreshDataClicked()
        {
            await AchievementsService.Instance.RefreshAsync();
        }

        // Achievements
        private async void OnAchievementDataUpdated()
        {
            foreach (var item in achievementListItems)
            {
                if (null == item)
                    continue; 

                Destroy(item.gameObject);
            }
            achievementListItems.Clear();
            achievementDataList.Clear();

            uint achievementDefCount = AchievementsService.GetAchievementsCount();

            if (achievementDefCount > 0)
            {
                foreach (var achievementDef in AchievementsService.Instance.CachedAchievements())
                {
                    achievementDataList.Add(new AchievementData()
                    {
                        Definition = achievementDef,
                        PlayerData = null
                    });
                }

                var userId = EOSManager.Instance.GetProductUserId();
                if (userId.IsValid())
                {
                    foreach (var playerAch in AchievementsService.Instance.CachedPlayerAchievements(userId))
                    {
                        var achData = achievementDataList.Find((AchievementData data) => data.Definition.AchievementId == playerAch.AchievementId);
                        if (achData != null)
                        {
                            achData.PlayerData = playerAch;
                        }
                    }
                }

                scrollRect.gameObject.SetActive(true);
                scrollRect.content.sizeDelta = new Vector2(0, achievementDefCount * 30);

                List<AchievementData> achievementDataListCopy = new(achievementDataList);
                foreach (var achievementData in achievementDataListCopy)
                {
                    await AddAchievementButton(achievementData);
                }
            }
            else
            {
                definitionsDescription.text = "No Achievements Found";
                definitionsDescription.gameObject.SetActive(true);
            }

            RefreshDisplayingDefinition();
        }

        private async Task AddAchievementButton(AchievementData achievement)
        {
            string achievementId = achievement.Definition.AchievementId;
            
            var button = Instantiate(itemTemplate, achievementListContainer);
            button.SetNameText(achievementId);
            button.gameObject.SetActive(true);
            button.index = achievementListItems.Count;

            achievementListItems.Add(button);

            bool unlocked = achievement.PlayerData.HasValue && achievement.PlayerData.Value.Progress >= 1;

            Task<Texture2D> getIconTextureTask = unlocked
                ? AchievementsService.Instance.GetAchievementUnlockedIconTexture(achievementId)
                : AchievementsService.Instance.GetAchievementLockedIconTexture(achievementId);

            var tex = await getIconTextureTask;
            
            button.SetIconTexture(tex);
        }

        public void OnShowDefinitionChanged(bool value)
        {
            displayDefinition = value;

            if (displayIndex >= 0)
            {
                OnDefinitionIdButtonClicked(displayIndex);
            }
        }
        
        public void RefreshDisplayingDefinition()
        {
            if (displayIndex == -1)
            {
                return;
            }
            OnDefinitionIdButtonClicked(displayIndex);
        }

        public async void OnDefinitionIdButtonClicked(int i)
        {
            if (i > AchievementsService.GetAchievementsCount())
            {
                return;
            }

            displayIndex = i;

            var achievementData = achievementDataList[i];
            var definition = achievementData.Definition;
            achievementUnlockedIcon.texture = await AchievementsService.Instance.GetAchievementUnlockedIconTexture(definition.AchievementId);
            achievementLockedIcon.texture = await AchievementsService.Instance.GetAchievementLockedIconTexture(definition.AchievementId);

            unlockAchievementButton.gameObject.SetActive(true);

            if (displayDefinition)
            {
                DisplayAchievementDefinition(definition);
            }
            else
            {
                DisplayPlayerAchievement(definition);
            }

            unlockAchievementButton.interactable = achievementData.PlayerData.HasValue && achievementData.PlayerData?.Progress < 1;
        }

        //Show player-specific achievement data
        void DisplayPlayerAchievement(DefinitionV2 definition)
        {
            PlayerAchievement? achievementNullable = null;
            foreach (var ach in AchievementsService.Instance.CachedPlayerAchievements(EOSManager.Instance.GetProductUserId()))
            {
                if (ach.AchievementId == definition.AchievementId)
                {
                    achievementNullable = ach;
                    break;
                }
            }

            if (achievementNullable == null)
            {
                definitionsDescription.text = "Player achievement info not found.";
                definitionsDescription.gameObject.SetActive(true);
                achievementUnlockedIcon.gameObject.SetActive(true);
                achievementLockedIcon.gameObject.SetActive(true);
                return;
            }

            PlayerAchievement achievement = achievementNullable.Value;
            string selectedDescription = string.Format(
                "Id: {0}\nDisplay Name: {1}\nDescription: {2}\nProgress: {3}\nUnlock Time: {4}\n",
                achievement.AchievementId, achievement.DisplayName, achievement.Description, achievement.Progress, achievement.UnlockTime);

            if (achievement.StatInfo != null)
            {
                foreach (PlayerStatInfo si in achievement.StatInfo)
                {
                    selectedDescription += String.Format("Stat Info: '{0}': {1}/{2}\n", si.Name, si.CurrentValue, si.ThresholdValue);
                }
            }

            bool locked = achievement.Progress < 1.0;

            definitionsDescription.text = selectedDescription;
            definitionsDescription.gameObject.SetActive(true);
            achievementUnlockedIcon.gameObject.SetActive(!locked);
            achievementLockedIcon.gameObject.SetActive(locked);
        }

        //Show global achievement definition
        void DisplayAchievementDefinition(DefinitionV2 definition)
        {
            string selectedDescription = string.Format(
                "Id: {0}\nUnlocked Display Name: {1}\nUnlocked Description: {2}\nLocked Display Name: {3}\nLocked Description: {4}\nHidden: {5}\n",
                definition.AchievementId, definition.UnlockedDisplayName, definition.UnlockedDescription, definition.LockedDisplayName, definition.LockedDescription, definition.IsHidden);

            foreach (StatThresholds st in definition.StatThresholds)
            {
                selectedDescription += string.Format("Stat Thresholds: '{0}': {1}\n", st.Name, st.Threshold);
            }

            definitionsDescription.text = selectedDescription;
            definitionsDescription.gameObject.SetActive(true);
            achievementUnlockedIcon.gameObject.SetActive(true);
            achievementLockedIcon.gameObject.SetActive(true);
        }
    }
}