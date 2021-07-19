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

﻿using Epic.OnlineServices;
using Epic.OnlineServices.Leaderboards;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UILeaderboardMenu : MonoBehaviour
    {
        [Header("Leaderboard UI")]
        public GameObject LeaderboardUIParent;

        public GameObject LeaderboardDefinitionsContentParent;
        public GameObject UIFileNameEntryPrefab;

        public Text CurrentSelectedLeaderboardTxt;

        public InputField ingestStatValueInput;

        private string currentSelectedDefinitionLeaderboardId = string.Empty;
        private string currentSelectedDefinitionStatName = string.Empty;

        private EOSLeaderboardManager LeaderboardManager;
        private EOSFriendsManager PlayerManager;

        private void Start()
        {
            PlayerManager = EOSManager.Instance.GetOrCreateManager<EOSFriendsManager>();
            LeaderboardManager = EOSManager.Instance.GetOrCreateManager<EOSLeaderboardManager>();

            CurrentSelectedLeaderboardTxt.text = "*select definition*";
        }

        private void DefinitionListOnClick(string leaderboardId)
        {
            currentSelectedDefinitionLeaderboardId = leaderboardId;

            Definition leaderboard = LeaderboardManager.GetCachedDefinitionFromId(leaderboardId);

            if(leaderboard != null)
            {
                CurrentSelectedLeaderboardTxt.text = leaderboard.StatName;
                currentSelectedDefinitionStatName = leaderboard.StatName;
            }
            else
            {
                Debug.LogErrorFormat("UILeaderboardMenu (DefinitionListOnClick): CachedDefinition not found for LeaderboardId={0}", leaderboardId);
                return;
            }

            LeaderboardManager.QueryRanks(leaderboardId, QueryRanksCompleted);
        }

        private void QueryRanksCompleted(Result result)
        {
            if(result != Result.Success)
            {
                Debug.LogErrorFormat("UILeaderboardMenu (QueryRanksCompleted): returned result error: {0}", result);
                return;
            }

            // Update UI
            if(LeaderboardManager.GetCachedLeaderboardRecords(out List<LeaderboardRecord> leaderboardRecords))
            {
                Debug.LogFormat("Display Leaderboard Records: Count={0}", leaderboardRecords.Count);

                foreach(LeaderboardRecord record in leaderboardRecords)
                {
                    Debug.LogFormat("    Record: UserName={0} ({1}), Rank={2}, Score={3} ", record.UserDisplayName, record.UserId, record.Rank, record.Score);
                }
            }
        }

        public void RefreshDefinitionsOnClick()
        {
            LeaderboardManager.QueryDefinitions(RefreshCachedDefinitions);
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
                        uiEntry.ShowSelectedColor();

                        // TODO: Update Ranks/UserScores
                    }
                }
            }
        }

        public void ShowFriendsOnClick()
        {
            if (!string.IsNullOrEmpty(currentSelectedDefinitionLeaderboardId))
            {
                List<ProductUserId> friends = new List<ProductUserId>();

                PlayerManager.GetCachedFriends(out Dictionary<EpicAccountId, FriendData> cachedFriends);

                foreach (FriendData friendData in cachedFriends.Values)
                {
                    friends.Add(friendData.UserProductUserId);
                }

                if(friends.Count == 0)
                {
                    Debug.LogWarning("UILeaderboardMenu (ShowFriendsOnClick): No friends found.");
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
                return;
            }

            // Update UI
            if (LeaderboardManager.GetCachedLeaderboardUserScores(out Dictionary<string, List<LeaderboardUserScore>> leaderboardUserScores))
            {
                // key == leaderboardId

                Debug.LogFormat("  Display LeaderboardId entries: Count={0}", leaderboardUserScores.Count);

                foreach (KeyValuePair<string, List<LeaderboardUserScore>> kvp in leaderboardUserScores)
                {
                    Debug.LogFormat("  Display LeaderboardId={0}, UserScores: Count={1}", kvp.Key, kvp.Value.Count);

                    foreach (LeaderboardUserScore userScore in kvp.Value)
                    {
                        Debug.LogFormat("    UserScore: UserId={0}, Score={3}", userScore.UserId, userScore.Score);
                    }
                }
            }
        }

        public void ShowGlobalOnClick()
        {
            if (!string.IsNullOrEmpty(currentSelectedDefinitionLeaderboardId))
            {
                LeaderboardManager.QueryRanks(currentSelectedDefinitionLeaderboardId, QueryRanksCompleted);
            }
            else
            {
                Debug.LogError("UILeaderboardMenu (ShowGlobalOnClick): No leaderboard selected.");
            }
        }

        public void IngestStatOnClick()
        {
            if(string.IsNullOrEmpty(ingestStatValueInput.text))
            {
                Debug.LogWarning("UILeaderboardMenu (IngestStatOnClick): Input a value.");
                return;
            }

            if(!Int32.TryParse(ingestStatValueInput.text, out int amount))
            {
                Debug.LogWarning("UILeaderboardMenu (IngestStatOnClick): Input is not a valid integer.");
                return;
            }

            if (string.IsNullOrEmpty(currentSelectedDefinitionStatName))
            {
                Debug.LogError("UILeaderboardMenu (IngestStatOnClick): StatName is null! Select a Definition or problems finding cached definition (refresh).");
                return;
            }

            LeaderboardManager.IngestStat(currentSelectedDefinitionStatName, amount);
        }

        public void ShowMenu()
        {
            EOSManager.Instance.GetOrCreateManager<EOSLeaderboardManager>().OnLoggedIn();

            LeaderboardUIParent.gameObject.SetActive(true);
        }

        public void HideMenu()
        {
            LeaderboardManager?.OnLoggedOut();

            LeaderboardUIParent.gameObject.SetActive(false);
        }
    }
}