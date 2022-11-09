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
using UnityEngine.UI;

using Epic.OnlineServices;
using Epic.OnlineServices.Mods;

using PlayEveryWare.EpicOnlineServices;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UIModEntry : MonoBehaviour
    {
        public Text Title;
        public Text Version;
        public Text NamespaceId;
        public Text ItemId;
        public Text ArtifactId;

        // Callbacks
        public Action<ModIdentifier> InstallOnClick;
        public Action<ModIdentifier> UpdateOnClick;
        public Action<ModIdentifier> UninstallOnClick;

        ModIdentifier modIdentifier;
        //public void UpdateModData(ModIdentifier identifier)
        //{
        //}

        public void UpdateUI()
        {
            Title.text = modIdentifier.Title;
            Version.text = modIdentifier.Version;
            NamespaceId.text = modIdentifier.NamespaceId;
            ItemId.text = modIdentifier.ItemId;
            ArtifactId.text = modIdentifier.ArtifactId;
        }

        public void ModEntryInstallOnClick()
        {
            if (InstallOnClick != null)
            {
                InstallOnClick(modIdentifier);
            }
            else
            {
                Debug.LogError("ModEntryInstallOnClick: InstallOnClick action is null!");
            }
        }

        public void ModEntryUpdateOnClick()
        {
            if (UpdateOnClick != null)
            {
                UpdateOnClick(modIdentifier);
            }
            else
            {
                Debug.LogError("ModEntryUpdateOnClick: UpdateOnClick action is null!");
            }
        }

        public void ModEntryUninstallOnClick()
        {
            if (UninstallOnClick != null)
            {
                UninstallOnClick(modIdentifier);
            }
            else
            {
                Debug.LogError("ModEntryUninstallOnClick: UninstallOnClick action is null!");
            }
        }
    }
}