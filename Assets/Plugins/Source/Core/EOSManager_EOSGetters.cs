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

using UnityEngine;

#if !EOS_DISABLE

namespace PlayEveryWare.EpicOnlineServices
{
    public partial class EOSManager : MonoBehaviour
    {
        public partial class EOSSingleton
        {

            //-------------------------------------------------------------------------j
            /// <summary>
            /// Helper to get the Auth interface.
            /// </summary>
            /// <returns></returns>
            public Epic.OnlineServices.Auth.AuthInterface GetEOSAuthInterface()
            {
                return GetEOSPlatformInterface().GetAuthInterface();
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Helper to get the Achievements interface.
            /// </summary>
            /// <returns></returns>
            public Epic.OnlineServices.Achievements.AchievementsInterface GetEOSAchievementInterface()
            {
                return GetEOSPlatformInterface().GetAchievementsInterface();
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Helper to get the Connect interface.
            /// </summary>
            /// <returns></returns>
            public Epic.OnlineServices.Connect.ConnectInterface GetEOSConnectInterface()
            {
                return GetEOSPlatformInterface().GetConnectInterface();
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Helper to get the Ecom interface.
            /// </summary>
            /// <returns></returns>
            public Epic.OnlineServices.Ecom.EcomInterface GetEOSEcomInterface()
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogError("Attempting to grab the EComInterface in the Editor: Won't work because the overlay isn't supported in the Unity Editor");
#endif

                return GetEOSPlatformInterface().GetEcomInterface();
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Helper to get the Friends interface.
            /// </summary>
            /// <returns></returns>
            public Epic.OnlineServices.Friends.FriendsInterface GetEOSFriendsInterface()
            {
                return GetEOSPlatformInterface().GetFriendsInterface();
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Helper to get the Leaderboards interface.
            /// </summary>
            /// <returns></returns>
            public Epic.OnlineServices.Leaderboards.LeaderboardsInterface GetEOSLeaderboardsInterface()
            {
                return GetEOSPlatformInterface().GetLeaderboardsInterface();
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Helper to get the Lobby interface.
            /// </summary>
            /// <returns></returns>
            public Epic.OnlineServices.Lobby.LobbyInterface GetEOSLobbyInterface()
            {
                return GetEOSPlatformInterface().GetLobbyInterface();
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Helper to get the Metrics interface.
            /// </summary>
            /// <returns></returns>
            public Epic.OnlineServices.Metrics.MetricsInterface GetEOSMetricsInterface()
            {
                return GetEOSPlatformInterface().GetMetricsInterface();
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Helper to get the Mods interface.
            /// </summary>
            /// <returns></returns>
            public Epic.OnlineServices.Mods.ModsInterface GetEOSModsInterface()
            {
                return GetEOSPlatformInterface().GetModsInterface();
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Helper to get the P2P interface.
            /// </summary>
            /// <returns></returns>
            public Epic.OnlineServices.P2P.P2PInterface GetEOSP2PInterface()
            {
                return GetEOSPlatformInterface().GetP2PInterface();
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Helper to get the PlayerDataStorage interface.
            /// </summary>
            /// <returns></returns>
            public Epic.OnlineServices.PlayerDataStorage.PlayerDataStorageInterface GetPlayerDataStorageInterface()
            {
                var playerDataStorageInterface = GetEOSPlatformInterface().GetPlayerDataStorageInterface();

                if (playerDataStorageInterface == null)
                {
                    throw new System.Exception("Could not get PlayerDataStorage interface, EncryptionKey may be empty or null");
                }
                return playerDataStorageInterface;
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Helper to get the Presence interface.
            /// </summary>
            /// <returns></returns>
            public Epic.OnlineServices.Presence.PresenceInterface GetEOSPresenceInterface()
            {
                return GetEOSPlatformInterface().GetPresenceInterface();
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Helper to get the RTC interface.
            /// </summary>
            /// <returns></returns>
            public Epic.OnlineServices.RTC.RTCInterface GetEOSRTCInterface()
            {
                return GetEOSPlatformInterface().GetRTCInterface();
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Helper to get the Sessions interface.
            /// </summary>
            /// <returns></returns>
            public Epic.OnlineServices.Sessions.SessionsInterface GetEOSSessionsInterface()
            {
                return GetEOSPlatformInterface().GetSessionsInterface();
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Helper to get the Stats interface.
            /// </summary>
            /// <returns></returns>
            public Epic.OnlineServices.Stats.StatsInterface GetEOSStatsInterface()
            {
                return GetEOSPlatformInterface().GetStatsInterface();
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Helper to get the TitleStorage interface.
            /// </summary>
            /// <returns></returns>
            public Epic.OnlineServices.TitleStorage.TitleStorageInterface GetEOSTitleStorageInterface()
            {
                return GetEOSPlatformInterface().GetTitleStorageInterface();
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Helper to get the UI interface.
            /// </summary>
            /// <returns></returns>
            public Epic.OnlineServices.UI.UIInterface GetEOSUIInterface()
            {
                return GetEOSPlatformInterface().GetUIInterface();
            }

            //-------------------------------------------------------------------------
            /// <summary>
            /// Helper to get the UserInfo interface.
            /// </summary>
            /// <returns></returns>
            public Epic.OnlineServices.UserInfo.UserInfoInterface GetEOSUserInfoInterface()
            {
                return GetEOSPlatformInterface().GetUserInfoInterface();
            }
        }
    }
}

#endif