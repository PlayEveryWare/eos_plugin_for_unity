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
    using System.Collections.Generic;

    using UnityEngine;

    using Epic.OnlineServices;
    using Epic.OnlineServices.UserInfo;
    using Epic.OnlineServices.Auth;

    /// <summary>
    /// Class <c>EOSUserInfoManager</c> is a general purpose access point for user info, including local user.
    /// </summary>
    public class EOSUserInfoManager : IEOSSubManager, IConnectInterfaceEventListener, IAuthInterfaceEventListener
    {
        private UserInfoData LocalUserInfo;

        private UserInfoInterface UserInfoHandle;

        public delegate void OnUserInfoChangedCallback(UserInfoData NewUserInfo);
        public delegate void OnUserInfoQueryIdCallback(EpicAccountId UserId, Result QueryResult);
        public delegate void OnUserInfoQueryDisplayNameCallback(string DisplayName, Result QueryResult);

        private Dictionary<EpicAccountId, UserInfoData> UserInfoIdMapping;
        private Dictionary<string, UserInfoData> UserInfoDisplayNameMapping;

        private List<OnUserInfoChangedCallback> LocalUserInfoChangedCallbacks;

        public EOSUserInfoManager()
        {
            UserInfoHandle = EOSManager.Instance?.GetEOSPlatformInterface()?.GetUserInfoInterface();
            UserInfoIdMapping = new Dictionary<EpicAccountId, UserInfoData>();
            UserInfoDisplayNameMapping = new Dictionary<string, UserInfoData>();
            LocalUserInfoChangedCallbacks = new List<OnUserInfoChangedCallback>();
            UpdateLocalUserInfo();
        }

        public void OnConnectLogin(Epic.OnlineServices.Connect.LoginCallbackInfo loginCallbackInfo)
        {
            UserInfoHandle = EOSManager.Instance?.GetEOSPlatformInterface()?.GetUserInfoInterface();
            UpdateLocalUserInfo();
        }

        public void OnAuthLogin(Epic.OnlineServices.Auth.LoginCallbackInfo loginCallbackInfo)
        {
            UserInfoHandle = EOSManager.Instance?.GetEOSPlatformInterface()?.GetUserInfoInterface();
            UpdateLocalUserInfo();
        }

        public void OnAuthLogout(LogoutCallbackInfo logoutCallbackInfo)
        {
            LocalUserInfo = default;
            foreach (var callback in LocalUserInfoChangedCallbacks)
            {
                callback?.Invoke(LocalUserInfo);
            }
        }

        public void AddNotifyLocalUserInfoChanged(OnUserInfoChangedCallback Callback)
        {
            LocalUserInfoChangedCallbacks.Add(Callback);
        }

        public void RemoveNotifyLocalUserInfoChanged(OnUserInfoChangedCallback Callback)
        {
            LocalUserInfoChangedCallbacks.Remove(Callback);
        }

        public UserInfoData GetLocalUserInfo()
        {
            return LocalUserInfo;
        }

        public void ClearUserInfo()
        {
            UserInfoIdMapping.Clear();
            UserInfoDisplayNameMapping.Clear();
            if (LocalUserInfo.UserId.IsValid())
            {
                //at minimum, local user info should be cached
                UserInfoIdMapping.Add(LocalUserInfo.UserId, LocalUserInfo);
                if (!string.IsNullOrEmpty(LocalUserInfo.DisplayName))
                {
                    UserInfoDisplayNameMapping.Add(LocalUserInfo.DisplayName, LocalUserInfo);
                }
            }
        }

        private void UpdateLocalUserInfo()
        {
            if (UserInfoHandle == null)
            {
                return;
            }

            var userId = EOSManager.Instance.GetLocalUserId();
            if (userId?.IsValid() == true)
            {
                QueryUserInfoById(userId);
            }
        }

        public UserInfoData GetUserInfoById(EpicAccountId UserId)
        {
            UserInfoIdMapping.TryGetValue(UserId, out UserInfoData userInfo);
            return userInfo;
        }

        public UserInfoData GetUserInfoByDisplayName(string DisplayName)
        {
            UserInfoDisplayNameMapping.TryGetValue(DisplayName, out UserInfoData userInfo);
            return userInfo;
        }

        public void QueryUserInfoById(EpicAccountId UserId, OnUserInfoQueryIdCallback Callback = null)
        {
            if(UserId?.IsValid() != true)
            {
                Debug.LogError("UserInfo (QueryUserInfoById): Invalid UserId");
                Callback?.Invoke(UserId, Result.InvalidUser);
                return;
            }

            QueryUserInfoOptions options = new QueryUserInfoOptions()
            {
                LocalUserId = EOSManager.Instance.GetLocalUserId(),
                TargetUserId = UserId
            };

            UserInfoHandle.QueryUserInfo(ref options, Callback, OnQueryUserInfoIdCompleted);
        }

        private void OnQueryUserInfoIdCompleted(ref QueryUserInfoCallbackInfo data)
        {
            OnUserInfoQueryIdCallback Callback = data.ClientData as OnUserInfoQueryIdCallback;

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("UserData (OnQueryUserInfoCompleted): Error calling QueryUserInfo: {0}", data.ResultCode);
                Callback?.Invoke(data.TargetUserId, data.ResultCode);
                return;
            }

            Debug.Log("UserData (OnQueryUserInfoCompleted): QueryUserInfo successful");

            CopyUserInfoOptions options = new CopyUserInfoOptions()
            {
                LocalUserId = data.LocalUserId,
                TargetUserId = data.TargetUserId
            };

            Result result = UserInfoHandle.CopyUserInfo(ref options, out UserInfoData? userInfo);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("UserData (OnQueryUserInfoCompleted): CopyUserInfo error: {0}", result);
                Callback?.Invoke(data.TargetUserId, result);
                return;
            }

            if (userInfo != null)
            {
                AddUserInfo((UserInfoData)userInfo);
            }

            Callback?.Invoke(data.TargetUserId, data.ResultCode);
        }

        public void QueryUserInfoByDisplayName(string DisplayName, OnUserInfoQueryDisplayNameCallback Callback = null)
        {
            if (string.IsNullOrEmpty(DisplayName))
            {
                Debug.LogError("UserInfo (QueryUserInfoByDisplayName): Invalid Display Name");
                Callback?.Invoke(DisplayName, Result.InvalidParameters);
                return;
            }

            QueryUserInfoByDisplayNameOptions options = new QueryUserInfoByDisplayNameOptions()
            {
                LocalUserId = EOSManager.Instance.GetLocalUserId(),
                DisplayName = DisplayName
            };

            UserInfoHandle.QueryUserInfoByDisplayName(ref options, Callback, OnQueryUserInfoDisplayNameCompleted);
        }

        private void OnQueryUserInfoDisplayNameCompleted(ref QueryUserInfoByDisplayNameCallbackInfo data)
        {
            OnUserInfoQueryDisplayNameCallback Callback = data.ClientData as OnUserInfoQueryDisplayNameCallback;

            //QueryUserInfoByDisplayNameCallbackInfo.DisplayName memory is only valid within callback scope, so copy for safety
            string displayNameCopy = string.Copy(data.DisplayName);

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("UserData (OnQueryUserInfoDisplayNameCompleted): Error calling QueryUserInfoByDisplayName: {0}", data.ResultCode);
                Callback?.Invoke(displayNameCopy, data.ResultCode);
                return;
            }

            Debug.Log("UserData (OnQueryUserInfoDisplayNameCompleted): QueryUserInfoByDisplayName successful");

            CopyUserInfoOptions options = new CopyUserInfoOptions()
            {
                LocalUserId = data.LocalUserId,
                TargetUserId = data.TargetUserId
            };

            Result result = UserInfoHandle.CopyUserInfo(ref options, out UserInfoData? userInfo);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("UserData (OnQueryUserInfoDisplayNameCompleted): CopyUserInfo error: {0}", result);
                Callback?.Invoke(displayNameCopy, result);
                return;
            }

            if (userInfo != null)
            {
                AddUserInfo((UserInfoData)userInfo);
            }

            Callback?.Invoke(displayNameCopy, data.ResultCode);
        }

        private void AddUserInfo(UserInfoData UserInfo)
        {
            UserInfoIdMapping[UserInfo.UserId] = UserInfo;
            if (!string.IsNullOrEmpty(UserInfo.DisplayName))
            {
                UserInfoDisplayNameMapping[UserInfo.DisplayName] = UserInfo;
            }

            if (UserInfo.UserId == EOSManager.Instance.GetLocalUserId())
            {
                LocalUserInfo = UserInfo;
                foreach (var callback in LocalUserInfoChangedCallbacks)
                {
                    callback?.Invoke(LocalUserInfo);
                }
            }
        }
    }

}