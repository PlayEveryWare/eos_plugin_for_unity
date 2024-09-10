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
    using Epic.OnlineServices;
    using Epic.OnlineServices.Reports;
    using Epic.OnlineServices.Sanctions;

    using UnityEngine;

    public struct Sanction
    {
        public string Action;
        public DateTime TimePlaced;
    }

    /// <summary>Class <c>EOSReportsManager</c> is a simplified wrapper for EOS [Reports Interface](https://dev.epicgames.com/docs/services/en-US/GameServices/ReportsInterface/index.html) & [Sanctions Interface](https://dev.epicgames.com/docs/services/en-US/GameServices/SanctionsInterface/index.html).</summary>
    public class EOSReportsManager : IEOSSubManager
    {
        private Dictionary<ProductUserId, List<Sanction>> CachedPlayerSanctions;
        private bool CachedPlayerSanctionsDirty;

        // Manager Callbacks
        private OnSanctionsCallback QueryActivePlayerSanctionsCallback;

        public delegate void OnSanctionsCallback(Result result);

        public EOSReportsManager()
        {
            CachedPlayerSanctions = new Dictionary<ProductUserId, List<Sanction>>();
            CachedPlayerSanctionsDirty = true;

            QueryActivePlayerSanctionsCallback = null;
        }

        public void SendPlayerBehaviorReport(ProductUserId targetPlayer, PlayerReportsCategory category, string message = "")
        {
            if (!targetPlayer.IsValid())
            {
                Debug.LogError("Reports (SendPlayerBehaviorReport): targetPlayer is not valid!");
                return;
            }

            SendPlayerBehaviorReportOptions reportOptions = new SendPlayerBehaviorReportOptions()
            {
                ReporterUserId = EOSManager.Instance.GetProductUserId(),
                ReportedUserId = targetPlayer,
                Category = category
            };

            if (!string.IsNullOrEmpty(message))
            {
                reportOptions.Message = message;
            }

            ReportsInterface reportsHandle = EOSManager.Instance.GetEOSPlatformInterface().GetReportsInterface();
            reportsHandle.SendPlayerBehaviorReport(ref reportOptions, null, OnSendPlayerBehaviorReportCompleted);
        }

        private void OnSendPlayerBehaviorReportCompleted(ref SendPlayerBehaviorReportCompleteCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Report (OnSendPlayerBehaviorReportCompleted):  data == null!");
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Reports (OnSendPlayerBehaviorReportCompleted): error: {0}", data.ResultCode);
                return;
            }

            Debug.Log("Reports (OnSendPlayerBehaviorReportCompleted): Player report sent successfully.");
        }

        public void QueryActivePlayerSanctions(ProductUserId targetPlayer, OnSanctionsCallback QueryActivePlayerSanctionsCompleted)
        {
            SanctionsInterface sanctionsHandle = EOSManager.Instance.GetEOSPlatformInterface().GetSanctionsInterface();

            QueryActivePlayerSanctionsOptions queryOptions = new QueryActivePlayerSanctionsOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                TargetUserId = targetPlayer
            };

            QueryActivePlayerSanctionsCallback = QueryActivePlayerSanctionsCompleted;

            Debug.Log("Reports (QueryActivePlayerSanctions): Running query...");

            sanctionsHandle.QueryActivePlayerSanctions(ref queryOptions, null, OnQueryActivePlayerSanctionsCompleted);
        }

        private void OnQueryActivePlayerSanctionsCompleted(ref QueryActivePlayerSanctionsCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Sanctions (OnQueryActivePlayerSanctionsCompleted):  data == null!");
            //    QueryActivePlayerSanctionsCallback?.Invoke(Result.InvalidState);
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Sanctions (OnQueryActivePlayerSanctionsCompleted): error: {0}", data.ResultCode);
                QueryActivePlayerSanctionsCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.Log("Reports (OnQueryActivePlayerSanctionsCompleted): Query completed!");

            // CachePlayerSanctions

            SanctionsInterface sanctionsHandle = EOSManager.Instance.GetEOSPlatformInterface().GetSanctionsInterface();

            GetPlayerSanctionCountOptions countOptions = new GetPlayerSanctionCountOptions();
            countOptions.TargetUserId = data.TargetUserId;

            List<Sanction> sanctionList = new List<Sanction>();

            uint sanctionCount = sanctionsHandle.GetPlayerSanctionCount(ref countOptions);
            for(uint sanctionIndex = 0; sanctionIndex < sanctionCount; sanctionIndex++)
            {
                CopyPlayerSanctionByIndexOptions copyOptions = new CopyPlayerSanctionByIndexOptions()
                {
                    SanctionIndex = sanctionIndex,
                    TargetUserId = data.TargetUserId
                };

                Result result = sanctionsHandle.CopyPlayerSanctionByIndex(ref copyOptions, out PlayerSanction? sanction);

                if(result != Result.Success)
                {
                    Debug.LogErrorFormat("Sanctions (OnQueryActivePlayerSanctionsCompleted): CopyPlayerSanctionByIndex failed '{0}'", result);
                    break;
                }

                sanctionList.Add(
                    new Sanction() {
                        Action = sanction?.Action,
                        TimePlaced = DateTimeOffset.FromUnixTimeSeconds((long)(sanction?.TimePlaced)).DateTime
                    });
            }

            if (CachedPlayerSanctions.ContainsKey(data.TargetUserId))
            {
                CachedPlayerSanctions[data.TargetUserId] = sanctionList;
            }
            else
            {
                CachedPlayerSanctions.Add(data.TargetUserId, sanctionList);
            }

            CachedPlayerSanctionsDirty = true;
            QueryActivePlayerSanctionsCallback?.Invoke(Result.Success);
        }

        /// <summary>Returns cached Player Sanctions Dictionary where ProductUserId is the key and List(<c>Struct</c>) is the value.</summary>
        /// <param name="PlayerSanctions">Out parameter returns Dictionary where ProductUserId is the key and List(<c>Struct</c>) is the value.</param>
        /// <returns>True if <c>CachedPlayerSanctions</c> has changed since last call.</returns>
        public bool GetCachedPlayerSanctions(out Dictionary<ProductUserId, List<Sanction>> PlayerSanctions)
        {
            PlayerSanctions = CachedPlayerSanctions;

            bool returnDirty = CachedPlayerSanctionsDirty;

            CachedPlayerSanctionsDirty = false;

            return returnDirty;
        }
    }
}
