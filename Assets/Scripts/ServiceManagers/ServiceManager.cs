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
 *
 * 
 *
 */

//#define ENABLE_DEBUG_EOSSTATSMANAGER

namespace PlayEveryWare.EpicOnlineServices
{
    using Epic.OnlineServices;
    using Epic.OnlineServices.Connect;
    using System;
    using System.Collections.Generic;

    public abstract class ServiceManager : IEOSOnConnectLogin, IEOSSubManager
    {
        protected IList<Action> _updateCallbacks = new List<Action>();

        public void OnConnectLogin(LoginCallbackInfo loginCallbackInfo)
        {
            if (loginCallbackInfo.ResultCode != Result.Success)
            {
                UnityEngine.Debug.Log($"Player login did not succeed. Result code: {Enum.GetName(typeof(Result), loginCallbackInfo.ResultCode)}");
                return;
            }
            OnPlayerLogin(loginCallbackInfo.LocalUserId);
        }

        protected abstract void OnPlayerLogin(ProductUserId productUserId);

        public abstract void Refresh();

        public void AddUpdateCallback(Action callback)
        {
            _updateCallbacks.Add(callback);
        }

        public void RemoveUpdateCallback(Action callback)
        {
            _updateCallbacks.Remove(callback);
        }
    }
}