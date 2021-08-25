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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Epic.OnlineServices;
using Epic.OnlineServices.Reports;

using UnityEngine;

namespace PlayEveryWare.EpicOnlineServices.Samples
{

    /// <summary>Class <c>EOSReportsManager</c> is a simplified wrapper for EOS [Reports Interface](https://dev.epicgames.com/docs/services/en-US/GameServices/ReportsInterface/index.html).</summary>
    public class EOSReportsManager : IEOSSubManager
    {
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
            reportsHandle.SendPlayerBehaviorReport(reportOptions, null, OnSendPlayerBehaviorReportCompleted);
        }

        private void OnSendPlayerBehaviorReportCompleted(SendPlayerBehaviorReportCompleteCallbackInfo data)
        {
            if (data == null)
            {
                Debug.LogError("Report (OnSendPlayerBehaviorReportCompleted):  data == null!");
                return;
            }

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("Reports (OnSendPlayerBehaviorReportCompleted): error: {0}", data.ResultCode);
                return;
            }

            Debug.Log("Reports (OnSendPlayerBehaviorReportCompleted): Player report sent successfully.");
        }
    }
}
