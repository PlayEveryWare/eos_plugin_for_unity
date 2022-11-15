using Epic.OnlineServices;
using Epic.OnlineServices.Leaderboards;
using PlayEveryWare.EpicOnlineServices.Samples;
using System.Collections.Generic;
using UnityEngine;

namespace PlayEveryWare.EpicOnlineServices.InventoryGame
{
    /// <summary>
    /// Handles the leaderboard retrieval and submission.
    /// </summary>
    public class InventoryLeaderboard : MonoBehaviour
    {
        private const string LeaderboardId = "InventoryGame";

        [SerializeField] private GameObject leaderboardEntriesParent;
        [SerializeField] private GameObject leaderboardEntryPrefab;

        private EOSLeaderboardManager LeaderboardManager;
        private EOSFriendsManager PlayerManager;
        private Definition? leaderboardDefinition;

        private void Start()
        {
            PlayerManager = EOSManager.Instance.GetOrCreateManager<EOSFriendsManager>();
            LeaderboardManager = EOSManager.Instance.GetOrCreateManager<EOSLeaderboardManager>();
        }

        private void OnDestroy()
        {
            EOSManager.Instance.RemoveManager<EOSFriendsManager>();
            EOSManager.Instance.RemoveManager<EOSLeaderboardManager>();
        }

        private void QueryRanksCompleted(Result result)
        {
            if (result != Result.Success)
            {
                Debug.LogErrorFormat($"InventoryLeaderboard (QueryRanksCompleted): returned result error: {result}");
                return;
            }

            // Update UI
            if (LeaderboardManager.GetCachedLeaderboardRecords(out List<LeaderboardRecord> leaderboardRecords))
            {
                Debug.LogFormat($"Display Leaderboard Records: Count={leaderboardRecords.Count}");

                // Destroy current entries
                foreach (Transform child in leaderboardEntriesParent.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }

                foreach (LeaderboardRecord record in leaderboardRecords)
                {
                    Debug.LogFormat($"    Record: UserName={record.UserDisplayName} ({record.UserId}), Rank={record.Rank}, Score={record.Score}");

                    // Display in UI
                    GameObject entryUIObj = Instantiate(leaderboardEntryPrefab, leaderboardEntriesParent.transform);
                    var uiEntry = entryUIObj.GetComponent<InventoryLeaderboardEntry>();

                    if (uiEntry != null)
                    {
                        uiEntry.Rank.text = record.Rank.ToString();
                        uiEntry.Username.text = record.UserDisplayName;
                        uiEntry.Score.text = record.Score.ToString();
                    }
                }
            }

            leaderboardDefinition = LeaderboardManager.GetCachedDefinitionFromId(LeaderboardId);
        }

        public void IngestNewScore(int amount)
        {
            LeaderboardManager.IngestStat(leaderboardDefinition.Value.StatName, amount);
            RefreshLeaderboard();
        }

        /// <summary>
        /// Refreshes the leaderboard information.
        /// This must be called at least once before submitting a score in order to the definition information.
        /// </summary>
        public void RefreshLeaderboard()
        {
            if (LeaderboardManager != null)
            {
                // Need to query the leaderboard first in order to pull down all the definitions.
                LeaderboardManager.QueryDefinitions(null);
                LeaderboardManager.QueryRanks(LeaderboardId, QueryRanksCompleted);
            }
        }

        public void ShowFriendsOnClick()
        {
            List<ProductUserId> friends = new List<ProductUserId>();

            PlayerManager.GetCachedFriends(out Dictionary<EpicAccountId, FriendData> cachedFriends);

            foreach (FriendData friendData in cachedFriends.Values)
            {
                friends.Add(friendData.UserProductUserId);
            }

            if (friends.Count == 0)
            {
                Debug.LogWarning("UILeaderboardMenu (ShowFriendsOnClick): No friends found.");

                // Destroy current entries
                foreach (Transform child in leaderboardEntriesParent.transform)
                {
                    Destroy(child.gameObject);
                }
            }
            else
            {
                LeaderboardManager.QueryUserScores(new List<string>() { LeaderboardId }, friends, QueryUserScoresCompleted);
            }
        }

        private void QueryUserScoresCompleted(Result result)
        {
            if (result != Result.Success)
            {
                Debug.LogErrorFormat($"UILeaderboardMenu (QueryUserScoresCompleted): returned result error: {result}");
                return;
            }

            // Update UI
            if (LeaderboardManager.GetCachedLeaderboardUserScores(out Dictionary<string, List<LeaderboardUserScore>> leaderboardUserScores))
            {
                // key == leaderboardId

                Debug.Log($"  Display LeaderboardId entries: Count={leaderboardUserScores.Count}");

                // Destroy current entries
                foreach (Transform child in leaderboardEntriesParent.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }

                foreach (KeyValuePair<string, List<LeaderboardUserScore>> kvp in leaderboardUserScores)
                {
                    Debug.Log($"  Display LeaderboardId={kvp.Key}, UserScores: Count={kvp.Value.Count}");

                    foreach (LeaderboardUserScore userScore in kvp.Value)
                    {
                        Debug.Log($"    UserScore: UserId={userScore.UserId}, Score={userScore.Score}");

                        // Display in UI
                        var copyResult = LeaderboardManager.CopyUserScore(userScore.UserId, out LeaderboardRecord? record);

                        GameObject entryUIObj = Instantiate(leaderboardEntryPrefab, leaderboardEntriesParent.transform);
                        var uiEntry = entryUIObj.GetComponent<InventoryLeaderboardEntry>();

                        if (uiEntry != null)
                        {
                            if (copyResult == Result.Success && record.HasValue)
                            {
                                uiEntry.Rank.text = record.Value.Rank.ToString();
                                uiEntry.Username.text = record.Value.UserDisplayName;
                                uiEntry.Score.text = record.Value.Score.ToString();
                            }
                            else
                            {
                                uiEntry.Rank.text = "-";
                                uiEntry.Username.text = userScore.UserId.ToString();
                                uiEntry.Score.text = userScore.Score.ToString();
                            }
                        }
                    }
                }
            }
        }
    }
}
