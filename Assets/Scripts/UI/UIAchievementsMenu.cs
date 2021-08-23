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
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

using Epic.OnlineServices;
using Epic.OnlineServices.Achievements;
using Epic.OnlineServices.Ecom;
using Epic.OnlineServices.UI;

using PlayEveryWare.EpicOnlineServices;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    /// <summary>
    /// Unity UI sample that uses <c>AchievementManager</c> to demo features.  Can be used as a template or starting point for implementing Achievement features.
    /// </summary>

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

        [Header("Controller")]
        public GameObject UIFirstSelected;

        private EOSAchievementManager achievementManager;

        public void Start()
        {
            HideMenu();
            achievementManager = EOSManager.Instance.GetOrCreateManager<EOSAchievementManager>();
        }

#if ENABLE_INPUT_SYSTEM
        private void Update()
        {
            // Controller: Detect if nothing is selected and controller input detected, and set default
            var gamepad = Gamepad.current;

            if (UIFirstSelected.activeSelf == true
                && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null
                && gamepad != null && gamepad.wasUpdatedThisFrame)
            {
                // Controller
                EventSystem.current.SetSelectedGameObject(UIFirstSelected);
                Debug.Log("Nothing currently selected, default to UIFirstSelected: EventSystem.current.currentSelectedGameObject = " + EventSystem.current.currentSelectedGameObject);
            }
        }
#endif

        public void ShowMenu()
        {
            getAchievementsButton.gameObject.SetActive(true);

            // Controller
            EventSystem.current.SetSelectedGameObject(UIFirstSelected);
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
                definitionsDescription.gameObject.SetActive(true);
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