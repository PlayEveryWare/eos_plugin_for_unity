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
    using UnityEngine.UI;
    using Epic.OnlineServices.UserInfo;
    using Epic.OnlineServices.Auth;

    public class UIDisplayName : MonoBehaviour, IAuthInterfaceEventListener, IConnectInterfaceEventListener
    {
        public Text DisplayNameText;

        private EOSUserInfoManager userInfoManager = null;

        const string NoUser = "No Local User";

        private void OnEnable()
        {
            EOSManager.Instance.AddConnectLoginListener(this);
            EOSManager.Instance.AddAuthLoginListener(this);
            EOSManager.Instance.AddAuthLogoutListener(this);

            DisplayNameText.text = NoUser;

            if (EOSManager.Instance.GetLocalUserId() != null)
            {
                OnLogin();
            }
        }

        private void OnDisable()
        {
            Clear();
        }

        private void Clear()
        {
            EOSManager.Instance.RemoveConnectLoginListener(this);
            EOSManager.Instance.RemoveAuthLoginListener(this);
            EOSManager.Instance.RemoveAuthLogoutListener(this);

            userInfoManager?.RemoveNotifyLocalUserInfoChanged(OnLocalUserInfoChanged);
            DisplayNameText.text = string.Empty;
        }

        private void OnLocalUserInfoChanged(UserInfoData UserInfo)
        {
            if (UserInfo.UserId?.IsValid() == true)
            {
                DisplayNameText.text = UserInfo.DisplayName;
            }
            else
            {
                DisplayNameText.text = NoUser;
            }
        }

        void OnLogin()
        {
            if (userInfoManager == null)
            {
                userInfoManager = EOSManager.Instance.GetOrCreateManager<EOSUserInfoManager>();
            }
            //remove callback first to avoid duplicating
            userInfoManager.RemoveNotifyLocalUserInfoChanged(OnLocalUserInfoChanged);
            userInfoManager.AddNotifyLocalUserInfoChanged(OnLocalUserInfoChanged);
            var userInfo = userInfoManager.GetLocalUserInfo();
            OnLocalUserInfoChanged(userInfo);
        }

        public void OnAuthLogout(LogoutCallbackInfo logoutCallbackInfo)
        {
            Clear();
        }

        public void OnAuthLogin(LoginCallbackInfo loginCallbackInfo)
        {
            OnLogin();
        }

        public void OnConnectLogin(Epic.OnlineServices.Connect.LoginCallbackInfo loginCallbackInfo)
        {
            OnLogin();
        }
    }
}