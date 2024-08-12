/*
 * Copyright (c) 2024 PlayEveryWare
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

namespace PlayEveryWare.EpicOnlineServices
{
    using Epic.OnlineServices.Auth;
    using System;
    using UnityEngine;

    public abstract class AuthenticationListener : MonoBehaviour, IAuthInterfaceEventListener, IConnectInterfaceEventListener
    {
        public void OnEnable()
        {
            EOSManager.Instance.AddAuthLoginListener(this);
            EOSManager.Instance.AddAuthLogoutListener(this);
            EOSManager.Instance.AddConnectLoginListener(this);
        }

        public void OnDisable()
        {
            EOSManager.Instance.RemoveAuthLoginListener(this);
            EOSManager.Instance.RemoveAuthLogoutListener(this);
            EOSManager.Instance.RemoveConnectLoginListener(this);
        }

        public void OnAuthLogin(LoginCallbackInfo loginCallbackInfo)
        {
            OnAuthenticationChanged(true);
        }

        public void OnAuthLogout(LogoutCallbackInfo logoutCallbackInfo)
        {
            OnAuthenticationChanged(false);
        }

        public void OnConnectLogin(Epic.OnlineServices.Connect.LoginCallbackInfo loginCallbackInfo)
        {
            OnAuthenticationChanged(true);
        }

        protected abstract void OnAuthenticationChanged(bool authenticated);
    }
}