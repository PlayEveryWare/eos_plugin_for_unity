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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Epic.OnlineServices;
using Epic.OnlineServices.Platform;
using Epic.OnlineServices.Mods;

namespace PlayEveryWare.EpicOnlineServices.Samples
{

    public class EOSModsManager : IEOSSubManager
    {
        private ModsInterface eosModsInterface;

        public ModInfo? allAvailableModsInfo;
        public ModInfo? installedModsInfo;

        bool installedModsCopied = false;
        bool allAvailableModsCopied = false;
        public EOSModsManager()
        {
        }

        [System.Diagnostics.Conditional("ENABLE_DEBUG_EOSMODSMANAGER")]
        static void print(string toPrint)
        {
            UnityEngine.Debug.Log(toPrint);
        }
        private PlatformInterface GetEOSPlatformInterface()
        {
            var eosPlatformInterface = EOSManager.Instance.GetEOSPlatformInterface();
            return eosPlatformInterface;
        }
        private ModsInterface GetEOSModsInterface()
        {
            if (eosModsInterface == null)
            {
                var eosPlatformInterface = EOSManager.Instance.GetEOSPlatformInterface();
                eosModsInterface = eosPlatformInterface.GetModsInterface();
            }
            return eosModsInterface;
        }

        public void CacheModsInfo()
        {
            installedModsCopied = false;
            allAvailableModsCopied = false;

            EnumerateMods(ModEnumerationType.Installed, (ref EnumerateModsCallbackInfo info) =>
            {
                if (info.ResultCode == Result.Success)
                {
                    CopyModInfo(ModEnumerationType.Installed, out installedModsInfo);
                    installedModsCopied = true;
                }
                else
                {
                    Debug.Log("Enumerate InstalledMods" + info.ResultCode);
                }
            });
            EnumerateMods(ModEnumerationType.AllAvailable, (ref EnumerateModsCallbackInfo info) =>
            {
                if (info.ResultCode == Result.Success)
                {
                    CopyModInfo(ModEnumerationType.AllAvailable, out allAvailableModsInfo);
                    allAvailableModsCopied = true;
                }
                else 
                {
                    Debug.Log("Enumerate AllAvailableMods" + info.ResultCode);
                }
            });
        }

        public void CopyModInfo(ModEnumerationType modEnumerationType, out ModInfo? modInfo)
        {
            var eosModsInterface = GetEOSModsInterface();
            var localUserId = EOSManager.Instance.GetLocalUserId();
            var options = new CopyModInfoOptions
            {
                LocalUserId = localUserId,
                Type = modEnumerationType
            };
            eosModsInterface.CopyModInfo(ref options, out modInfo);
        }
        public void EnumerateMods(ModEnumerationType modEnumerationType,OnEnumerateModsCallback callback)
        {
            var eosModsInterface = GetEOSModsInterface();
            var localUserId = EOSManager.Instance.GetLocalUserId();
            var options = new EnumerateModsOptions
            {
                LocalUserId = localUserId,
                Type = modEnumerationType
            };
            eosModsInterface.EnumerateMods(ref options, null, callback);
        }

        public void InstallMod(ModIdentifier mod, OnInstallModCallback callback = null)
        {
            var eosModsInterface = GetEOSModsInterface();
            var localUserId = EOSManager.Instance.GetLocalUserId();
            var options = new InstallModOptions
            {
                LocalUserId = localUserId,
                Mod = mod
            };
            eosModsInterface.InstallMod(ref options, null, callback);
        }

        public void UninstallMod(ModIdentifier mod, OnUninstallModCallback callback = null)
        {
            var eosModsInterface = GetEOSModsInterface();
            var localUserId = EOSManager.Instance.GetLocalUserId();
            var options = new UninstallModOptions
            {
                LocalUserId = localUserId,
                Mod = mod
            };
            eosModsInterface.UninstallMod(ref options, null, callback);
        }

        public void UpdateMod(ModIdentifier mod, OnUpdateModCallback callback = null)
        {
            var eosModsInterface = GetEOSModsInterface();
            var localUserId = EOSManager.Instance.GetLocalUserId();
            var options = new UpdateModOptions
            {
                LocalUserId = localUserId,
                Mod = mod
            };
            eosModsInterface.UpdateMod(ref options, null, callback);
        }
    }
}