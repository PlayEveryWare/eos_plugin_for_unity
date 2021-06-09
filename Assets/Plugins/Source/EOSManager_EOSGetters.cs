using UnityEngine;

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
                return GetEOSPlatformInterface().GetPlayerDataStorageInterface();
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
