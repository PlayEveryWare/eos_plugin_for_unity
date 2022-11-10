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
using Epic.OnlineServices.Mods;

using PlayEveryWare.EpicOnlineServices;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    /// <summary>
    /// Unity UI sample that uses <c>AchievementManager</c> to demo features.  Can be used as a template or starting point for implementing Achievement features.
    /// </summary>

    public class UIModsMenu : MonoBehaviour, ISampleSceneUI
    {
        [Header("Mods UI")]
        public GameObject modsPanel;

        [Header("Mods UI - Mod Entries")]
        public GameObject UIModEntryPrefab;
        public GameObject ModContentParent;

        [Header("Controller")]
        //public GameObject UIFirstSelected;

        private EOSModsManager modsManager;

        private List<UIModEntry> UIModEntries = new List<UIModEntry>();

        public bool showAllMods = false;

        private void Awake()
        {
            HideMenu();
            modsManager = EOSManager.Instance.GetOrCreateManager<EOSModsManager>();
        }

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }

        private void OnDestroy()
        {
            EOSManager.Instance.RemoveManager<EOSModsManager>();
        }

        private void ShowSelectedModSet()
        {
            UIModEntries.Clear();

            //Dummy Mod Entry For Testing
            GameObject a = Instantiate(UIModEntryPrefab, ModContentParent.transform);
            UIModEntry b = a.GetComponent<UIModEntry>();
            if (b != null)
            {
                b.InstallOnClick = InstallButtonOnClick;
                b.UpdateOnClick = UpdateButtonOnClick;
                b.UninstallOnClick = UninstallButtonOnClick;

                UIModEntries.Add(b);
            }

            ModInfo? modsInfoSet = showAllMods ? modsManager.allAvailableModsInfo : modsManager.installedModsInfo;
            if (modsInfoSet == null) { return; }
            foreach (ModIdentifier modIdentifier in modsInfoSet?.Mods)
            {
                GameObject modUIObj = Instantiate(UIModEntryPrefab, ModContentParent.transform);
                UIModEntry uiEntry = modUIObj.GetComponent<UIModEntry>();
                if (uiEntry != null)
                {
                    uiEntry.UpdateModData(modIdentifier);

                    uiEntry.InstallOnClick = InstallButtonOnClick;
                    uiEntry.UpdateOnClick = UpdateButtonOnClick;
                    uiEntry.UninstallOnClick = UninstallButtonOnClick;

                    UIModEntries.Add(uiEntry);
                }
            }
        }
        public void ShowMenu()
        {
            modsPanel.gameObject.SetActive(true);

            modsManager.CacheModsInfo();
            ShowSelectedModSet();
        }

        public void HideMenu()
        {
            modsPanel.gameObject.SetActive(false);
        }

        public void InstallButtonOnClick(ModIdentifier mod)
        {
            modsManager.InstallMod(mod);
        }

        public void UninstallButtonOnClick(ModIdentifier mod) 
        {
            modsManager.UninstallMod(mod);
        }

        public void UpdateButtonOnClick(ModIdentifier mod)
        {
            modsManager.UpdateMod(mod);
        }
    }
}