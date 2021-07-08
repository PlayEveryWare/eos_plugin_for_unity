using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Epic.OnlineServices.Achievements;
using Epic.OnlineServices;
using Epic.OnlineServices.UI;
using Epic.OnlineServices.Ecom;

using PlayEveryWare.EpicOnlineServices;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UIAchievementsMenu : MonoBehaviour
    {
        [Header("Store UI")]
        public Button getAchievementsButton;
        public Text definitionsDescription;
        public ScrollRect scrollRect;
        public Transform spawnPoint;
        public Button item;
        public RawImage achievementUnlockedIcon;
        public RawImage achievementLockedIcon;

        private EOSAchievementManager achievementManager;

        public void Start()
        {
            HideMenu();
            achievementManager = EOSManager.Instance.GetOrCreateManager<EOSAchievementManager>();
        }

        public void ShowMenu()
        {
            getAchievementsButton.gameObject.SetActive(true);
        }

        public void HideMenu()
        {
            getAchievementsButton.gameObject.SetActive(false);
            definitionsDescription.gameObject.SetActive(false);
            scrollRect.gameObject.SetActive(false);
            achievementUnlockedIcon.gameObject.SetActive(false);
            achievementLockedIcon.gameObject.SetActive(false);
        }

        // Achievments
        public void OnGetAchievmentsClick()
        {
            uint achievementDefCount = achievementManager.GetAchievementDefinitionCount();

            if (achievementDefCount > 1)
            {
                scrollRect.gameObject.SetActive(true);
                scrollRect.content.sizeDelta = new Vector2(0, achievementDefCount * 30);

                int i = 0;
                foreach (var achievementDef in achievementManager.EnumerateCachedAchievementDefinitions())
                {
                    Vector3 newPos = new Vector3(spawnPoint.localPosition.x, -i * 30, spawnPoint.localPosition.z);
                    var newButton = Instantiate<Button>(item, newPos, spawnPoint.rotation);
                    newButton.GetComponentInChildren<Text>().text = achievementDef.AchievementId;
                    newButton.transform.SetParent(scrollRect.content, false);
                    newButton.gameObject.SetActive(true);
                    var achievementButton = newButton.GetComponent<UIAchievementButton>();
                    achievementButton.index = i;
                    newButton.onClick.AddListener(() =>
                    {
                        OnDefinitionIdButtonClicked(achievementButton.index);
                    });
                    i += 1;
                }

            }
            else
            {
                definitionsDescription.text = "No Achievements Found";
            }
        }

        public void OnDefinitionIdButtonClicked(int i)
        {
            if (i > achievementManager.GetAchievementDefinitionCount())
            {
                return;
            }

            var definition = achievementManager.GetAchievementDefinitionAtIndex(i);
            achievementUnlockedIcon.texture = achievementManager.GetAchievementUnlockedIconTexture(definition.AchievementId);
            achievementLockedIcon.texture = achievementManager.GetAchievementLockedIconTexture(definition.AchievementId);

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