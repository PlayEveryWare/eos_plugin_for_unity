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

using Epic.OnlineServices;
using Epic.OnlineServices.Leaderboards;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Epic.OnlineServices.Stats;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UILeaderboardMenu : MonoBehaviour, ISampleSceneUI
    {
        private enum LeaderboardGroup
        {
            Global,
            Friends
        }

        [Header("Leaderboard UI")]
        public GameObject LeaderboardUIParent;

        public GameObject LeaderboardDefinitionsContentParent;
        public GameObject UIFileNameEntryPrefab;

        public Text CurrentSelectedLeaderboardTxt;

        public GameObject LeaderboardEntriesContentParent;
        public GameObject UILeaderboardEntryPrefab;

        public UIConsoleInputField ingestStatValueInput;

        [Header("Controller")]
        public GameObject UIFirstSelected;

        private string currentSelectedDefinitionLeaderboardId = string.Empty;
        private string currentSelectedDefinitionStatName = string.Empty;
        private LeaderboardGroup currentGroup = LeaderboardGroup.Global;

        private EOSLeaderboardManager LeaderboardManager;
        private EOSFriendsManager PlayerManager;

        /// <summary>
        /// If this value is greater than 0, then every time this number of seconds pass, the current viewed leaderboard will refresh.
        /// The internal timer for this resets whenever a leaderboard is queried.
        /// There is no way to determine when a successfully ingested stat has been fully processed, and that stat proliferated to the leaderboards in the back end.
        /// Periodic refreshing of the leaderboard is one way to get up to date information.
        /// Note that per user, you should never request stats more than 100 times a minute.
        /// </summary>
        public float SecondsBetweenLeaderboardRefreshes = 30;

        /// <summary>
        /// If this value is greater than 0, after a successful <see cref="IngestStatOnClick"/>, this amount of seconds will be waited before refreshing the current leaderboard.
        /// There's no promise that a successfully ingested stat means that it is proliferated across other Epic Online Services systems, like leaderboards.
        /// This delay is a guess at how long to reasonably wait before requerying the leaderboard to reflect the newly ingested stat.
        /// </summary>
        public float SecondsAfterStatIngestedToRefresh = 2;

        /// <summary>
        /// Reference to the coroutine for waiting for leaderboard refreshes.
        /// Having a reference to it allows for slightly more powerful control of the coroutine.
        /// </summary>
        private Coroutine refreshLeaderboardCoroutine { get; set; }

        private void Start()
        {
            PlayerManager = EOSManager.Instance.GetOrCreateManager<EOSFriendsManager>();
            LeaderboardManager = EOSManager.Instance.GetOrCreateManager<EOSLeaderboardManager>();

            CurrentSelectedLeaderboardTxt.text = "*select definition*";
        }

        private void OnDestroy()
        {
            EOSManager.Instance.RemoveManager<EOSFriendsManager>();
            EOSManager.Instance.RemoveManager<EOSLeaderboardManager>();
        }

        private void DefinitionListOnClick(string leaderboardId)
        {
            currentSelectedDefinitionLeaderboardId = leaderboardId;

            Definition? leaderboard = LeaderboardManager.GetCachedDefinitionFromId(leaderboardId);

            if(leaderboard != null)
            {
                currentSelectedDefinitionStatName = leaderboard?.StatName;
                SetCurrentLeaderboardDescription();
            }
            else
            {
                Debug.LogErrorFormat("UILeaderboardMenu (DefinitionListOnClick): CachedDefinition not found for LeaderboardId={0}", leaderboardId);
                return;
            }

            ShowGlobalOnClick();
        }

        private void QueryRanksCompleted(Result result)
        {
            if(result != Result.Success)
            {
                Debug.LogErrorFormat("UILeaderboardMenu (QueryRanksCompleted): returned result error: {0}", result);

                // Even if we failed, try to refresh the leaderboard again later
                if (refreshLeaderboardCoroutine != null)
                {
                    StopCoroutine(refreshLeaderboardCoroutine);
                }
                refreshLeaderboardCoroutine = StartCoroutine(RefreshCurrentLeaderboardAfterWait(SecondsBetweenLeaderboardRefreshes));

                return;
            }

            // Update UI
            if(LeaderboardManager.GetCachedLeaderboardRecords(out List<LeaderboardRecord> leaderboardRecords))
            {
                Debug.LogFormat("Display Leaderboard Records: Count={0}", leaderboardRecords.Count);

                // Destroy current entries
                foreach (Transform child in LeaderboardEntriesContentParent.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }

                foreach (LeaderboardRecord record in leaderboardRecords)
                {
                    Debug.LogFormat("    Record: UserName={0} ({1}), Rank={2}, Score={3} ", record.UserDisplayName, record.UserId, record.Rank, record.Score);

                    // Display in UI

                    GameObject entryUIObj = Instantiate(UILeaderboardEntryPrefab, LeaderboardEntriesContentParent.transform);

                    UILeaderboardEntry uiEntry = entryUIObj.GetComponent<UILeaderboardEntry>();

                    if(uiEntry != null)
                    {
                        uiEntry.RankTxt.text = record.Rank.ToString();
                        uiEntry.NameTxt.text = record.UserDisplayName;
                        uiEntry.ScoreTxt.text = record.Score.ToString();
                    }
                }

                if (refreshLeaderboardCoroutine != null)
                {
                    StopCoroutine(refreshLeaderboardCoroutine);
                }
                refreshLeaderboardCoroutine = StartCoroutine(RefreshCurrentLeaderboardAfterWait(SecondsBetweenLeaderboardRefreshes));
            }
        }

        public void RefreshDefinitionsOnClick()
        {
            if(LeaderboardManager != null)
            {
                LeaderboardManager.QueryDefinitions(RefreshCachedDefinitions);
            }
        }

        private void RefreshCachedDefinitions(Result result)
        {
            if (LeaderboardManager.GetCachedLeaderboardDefinitions(out Dictionary<string, Definition> leaderboardsDefinitions))
            {
                // Destroy current definition list
                foreach (Transform child in LeaderboardDefinitionsContentParent.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }

                foreach (KeyValuePair<string, Definition> kvp in leaderboardsDefinitions)
                {
                    GameObject definitionUIObj = Instantiate(UIFileNameEntryPrefab, LeaderboardDefinitionsContentParent.transform);
                    UIFileNameEntry uiEntry = definitionUIObj.GetComponent<UIFileNameEntry>();

                    uiEntry.FileNameTxt.text = kvp.Key;
                    uiEntry.FileNameOnClick = DefinitionListOnClick;

                    if (kvp.Key.Equals(currentSelectedDefinitionLeaderboardId, StringComparison.OrdinalIgnoreCase))
                    {
                        RefreshCurrentSelectedLeaderboard();
                    }
                }
            }
        }

        public void ShowFriendsOnClick()
        {
            if (!string.IsNullOrEmpty(currentSelectedDefinitionLeaderboardId))
            {
                currentGroup = LeaderboardGroup.Friends;
                SetCurrentLeaderboardDescription();

                List<ProductUserId> friends = new List<ProductUserId>();

                PlayerManager.GetCachedFriends(out Dictionary<EpicAccountId, FriendData> cachedFriends);

                foreach (FriendData friendData in cachedFriends.Values)
                {
                    friends.Add(friendData.UserProductUserId);
                }

                if(friends.Count == 0)
                {
                    Debug.LogWarning("UILeaderboardMenu (ShowFriendsOnClick): No friends found.");

                    // Destroy current entries
                    foreach (Transform child in LeaderboardEntriesContentParent.transform)
                    {
                        GameObject.Destroy(child.gameObject);
                    }
                }
                else
                {
                    LeaderboardManager.QueryUserScores(new List<string>() { currentSelectedDefinitionLeaderboardId }, friends, QueryUserScoresCompleted);
                }
            }
            else
            {
                Debug.LogError("UILeaderboardMenu (ShowFriendsOnClick): No leaderboard selected.");
            }
        }

        private void QueryUserScoresCompleted(Result result)
        {
            if (result != Result.Success)
            {
                Debug.LogErrorFormat("UILeaderboardMenu (QueryUserScoresCompleted): returned result error: {0}", result);

                // Even if we failed, try to refresh the leaderboard again later
                if (refreshLeaderboardCoroutine != null)
                {
                    StopCoroutine(refreshLeaderboardCoroutine);
                }
                refreshLeaderboardCoroutine = StartCoroutine(RefreshCurrentLeaderboardAfterWait(SecondsBetweenLeaderboardRefreshes));

                return;
            }

            // Update UI
            if (LeaderboardManager.GetCachedLeaderboardUserScores(out Dictionary<string, List<LeaderboardUserScore>> leaderboardUserScores))
            {
                // key == leaderboardId

                Debug.Log($"  Display LeaderboardId entries: Count={leaderboardUserScores.Count}");

                // Destroy current entries
                foreach (Transform child in LeaderboardEntriesContentParent.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }

                foreach (KeyValuePair<string, List<LeaderboardUserScore>> kvp in leaderboardUserScores)
                {
                    Debug.Log($"  Display LeaderboardId={kvp.Key}, UserScores: Count={kvp.Value.Count}");

                    // Check to make sure to only add entries from the correct leaderboard if available.
                    if (!string.IsNullOrEmpty(currentSelectedDefinitionLeaderboardId) && kvp.Key != currentSelectedDefinitionLeaderboardId)
                    {
                        continue;
                    }

                    foreach (LeaderboardUserScore userScore in kvp.Value)
                    {
                        Debug.Log($"    UserScore: UserId={userScore.UserId}, Score={userScore.Score}");

                        // Display in UI
                        var copyResult = LeaderboardManager.CopyUserScore(userScore.UserId, out LeaderboardRecord? record);

                        GameObject entryUIObj = Instantiate(UILeaderboardEntryPrefab, LeaderboardEntriesContentParent.transform);

                        UILeaderboardEntry uiEntry = entryUIObj.GetComponent<UILeaderboardEntry>();

                        if (uiEntry != null)
                        {
                            if (copyResult == Result.Success && record.HasValue)
                            {
                                uiEntry.RankTxt.text = record.Value.Rank.ToString();
                                uiEntry.NameTxt.text = record.Value.UserDisplayName;
                                uiEntry.ScoreTxt.text = record.Value.Score.ToString();
                            }
                            else
                            {
                                uiEntry.RankTxt.text = "-";
                                uiEntry.NameTxt.text = userScore.UserId.ToString();
                                uiEntry.ScoreTxt.text = userScore.Score.ToString();
                            }
                        }
                    }
                }

                if (refreshLeaderboardCoroutine != null)
                {
                    StopCoroutine(refreshLeaderboardCoroutine);
                }
                refreshLeaderboardCoroutine = StartCoroutine(RefreshCurrentLeaderboardAfterWait(SecondsBetweenLeaderboardRefreshes));
            }
        }

        public void ShowGlobalOnClick()
        {
            if (!string.IsNullOrEmpty(currentSelectedDefinitionLeaderboardId))
            {
                currentGroup = LeaderboardGroup.Global;
                SetCurrentLeaderboardDescription();
                LeaderboardManager.QueryRanks(currentSelectedDefinitionLeaderboardId, QueryRanksCompleted);
            }
            else
            {
                Debug.LogError("UILeaderboardMenu (ShowGlobalOnClick): No leaderboard selected.");
            }
        }

        public async void IngestStatOnClick()
        {
            if(string.IsNullOrEmpty(ingestStatValueInput.InputField.text))
            {
                Debug.LogWarning("UILeaderboardMenu (IngestStatOnClick): Input a value.");
                return;
            }

            if(!Int32.TryParse(ingestStatValueInput.InputField.text, out int amount))
            {
                Debug.LogWarning("UILeaderboardMenu (IngestStatOnClick): Input is not a valid integer.");
                return;
            }

            if (string.IsNullOrEmpty(currentSelectedDefinitionStatName))
            {
                Debug.LogError("UILeaderboardMenu (IngestStatOnClick): StatName is null! Select a Definition or problems finding cached definition (refresh).");
                return;
            }

            await StatsService.Instance.IngestStatAsync(currentSelectedDefinitionStatName, amount);

            if (refreshLeaderboardCoroutine != null)
            {
                StopCoroutine(refreshLeaderboardCoroutine);
            }
            refreshLeaderboardCoroutine = StartCoroutine(RefreshCurrentLeaderboardAfterWait(SecondsAfterStatIngestedToRefresh));
        }

        public void ShowMenu()
        {
            LeaderboardUIParent.gameObject.SetActive(true);

            //EOSManager.Instance.GetOrCreateManager<EOSLeaderboardManager>().OnLoggedIn();
            Invoke("InitFriends",0);

            // Controller
            EventSystem.current.SetSelectedGameObject(UIFirstSelected);
        }

        private void InitFriends()
        {
            PlayerManager.QueryFriends(null);
            RefreshDefinitionsOnClick();
        }

        public void HideMenu()
        {
            LeaderboardManager?.OnLoggedOut();

            LeaderboardUIParent.gameObject.SetActive(false);
        }

        private void SetCurrentLeaderboardDescription()
        {
            CurrentSelectedLeaderboardTxt.text = $"{currentSelectedDefinitionLeaderboardId} - " +
                $"{currentSelectedDefinitionStatName} - {Enum.GetName(typeof(LeaderboardGroup), currentGroup)}";
        }

        /// <summary>
        /// If a leaderboard is selected, re-query and re-display its values.
        /// </summary>
        public void RefreshCurrentSelectedLeaderboard()
        {
            if (string.IsNullOrEmpty(currentSelectedDefinitionLeaderboardId))
            {
                Debug.Log($"UILeaderboardMenu (RefreshCurrentSelectedLeaderboard): No leaderboard selected.");
                return;
            }

            if (currentGroup == LeaderboardGroup.Friends)
            {
                ShowFriendsOnClick();
            }
            else
            {
                ShowGlobalOnClick();
            }
        }

        /// <summary>
        /// Coroutine that yields for <see cref="SecondsBetweenLeaderboardRefreshes"/>, then calls RefreshCurrentSelectedLeaderboard.
        /// Caller needs to re-call this to refresh after delay again.
        /// </summary>
        private System.Collections.IEnumerator RefreshCurrentLeaderboardAfterWait(float secondsToWait)
        {
            if (secondsToWait <= 0)
            {
                // No-op
                yield break;
            }

            Debug.Log($"UILeaderboardMenu (RefreshCurrentLeaderboardAfterWait): Waiting {secondsToWait} seconds to refresh leaderboard...");

            yield return new WaitForSeconds(secondsToWait);

            Debug.Log($"UILeaderboardMenu (RefreshCurrentLeaderboardAfterWait): Waiting time has passed, refreshing leaderboard.");

            RefreshCurrentSelectedLeaderboard();
        }
    }
}