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

namespace PlayEveryWare.EpicOnlineServices
{
    using Epic.OnlineServices;
    using Epic.OnlineServices.Connect;
    using System;
    using UnityEngine;

    /// <summary>
    /// Contains common implementations for the IEOSOnConnectLogin interface.
    /// Not all instances where IEOSOnConnectLogin is implemented are replaced
    /// by this abstract class, because a gradual conversion will have a smaller
    /// impact.
    /// </summary>
    public abstract class EOSOnConnectLogin : IEOSOnConnectLogin
    {
        /// <summary>
        /// The product user id associated with the connect login that called
        /// the connect login callback for this manager.
        /// </summary>
        protected ProductUserId _productUserId;

        public void OnConnectLogin(LoginCallbackInfo loginCallbackInfo)
        {
            if (loginCallbackInfo.ResultCode == Result.Success)
            {
                _productUserId = loginCallbackInfo.LocalUserId;
            }
            else
            {
                Debug.LogError($"Connect login failed with result code: {Enum.GetName(typeof(Result), loginCallbackInfo.ResultCode)}");
            }
        }

        protected abstract void PostSuccessfulLogin();
    }
}