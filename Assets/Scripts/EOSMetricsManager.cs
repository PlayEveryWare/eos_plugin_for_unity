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
    using UnityEngine;
    using Epic.OnlineServices;
    using Epic.OnlineServices.Metrics;
    using Epic.OnlineServices.Auth;

    /// <summary>
    /// Class <c>EOSMetricsManager</c> is a simplified wrapper for EOS [Metrics Interface](https://dev.epicgames.com/docs/game-services/eos-metrics-interface).
    /// </summary>
    public class EOSMetricsManager : IEOSSubManager, IAuthInterfaceEventListener
    {
        bool sessionActive;
        MetricsInterface metricsHandle;

        public EOSMetricsManager()
        {
            sessionActive = false;
            metricsHandle = EOSManager.Instance.GetEOSMetricsInterface();
        }

        /// <summary>
        /// Checks if a metrics session is active
        /// </summary>
        /// <returns>True if an EOS Metrics sessions is active</returns>
        public bool IsSessionActive()
        {
            return sessionActive;
        }

        /// <summary>
        /// Wraps [EOS_Metrics_BeginPlayerSession](https://dev.epicgames.com/docs/api-ref/functions/eos-metrics-begin-player-session) functionality
        /// </summary>
        /// <param name="DisplayName">Display name to use for metrics tracking</param>
        /// <param name="ControllerType">Controller type for tracked user</param>
        /// <param name="ServerIp">IP of session server. NULL for local session.</param>
        /// <param name="SessionId">Custom string identifier for game session</param>
        public void BeginSession(string DisplayName, UserControllerType ControllerType = UserControllerType.Unknown, string ServerIp = null, string SessionId = null)
        {
            if (sessionActive)
            {
                Debug.LogError("EOSMetricsManager (BeginSession): Session already active");
            }

            var options = new BeginPlayerSessionOptions();

            var accountId = new BeginPlayerSessionOptionsAccountId()
            {
                Epic = EOSManager.Instance.GetLocalUserId()
            };
            options.AccountId = accountId;

            options.DisplayName = DisplayName;
            options.ControllerType = ControllerType;
            options.ServerIp = ServerIp;
            options.GameSessionId = SessionId;

            var result = metricsHandle.BeginPlayerSession(ref options);
            if (result == Result.Success)
            {
                Debug.Log("EOSMetricsManager (BeginSession): Session started");
                sessionActive = true;
            }
            else
            {
                Debug.Log("EOSMetricsManager (BeginSession): Failed to start session: "+result.ToString());
            }
        }

        /// <summary>
        /// Wraps [EOS_Metrics_EndPlayerSession](https://dev.epicgames.com/docs/api-ref/functions/eos-metrics-end-player-session) functionality
        /// </summary>
        public void EndSession()
        {
            if (!sessionActive)
            {
                Debug.LogError("EOSMetricsManager (EndSession): Session not active");
            }

            var options = new EndPlayerSessionOptions();
            var accountId = new EndPlayerSessionOptionsAccountId()
            {
                Epic = EOSManager.Instance.GetLocalUserId()
            };
            options.AccountId = accountId;

            var result = metricsHandle.EndPlayerSession(ref options);
            if (result == Result.Success)
            {
                Debug.Log("EOSMetricsManager (EndSession): Session ended");
                sessionActive = false;
            }
            else
            {
                Debug.Log("EOSMetricsManager (EndSession): Failed to end session: " + result.ToString());
            }
        }

        public void OnAuthLogin(LoginCallbackInfo loginCallbackInfo)
        {
            // Default behavior for the time being is to take no action
        }

        public void OnAuthLogout(LogoutCallbackInfo logoutCallbackInfo)
        {
            if (sessionActive)
            {
                EndSession();
            }
        }
    }
}
