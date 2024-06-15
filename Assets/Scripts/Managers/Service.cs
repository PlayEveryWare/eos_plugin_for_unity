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

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using Epic.OnlineServices;
    using Epic.OnlineServices.Connect;
    using System;
    using System.Collections.Generic;

    public abstract class Service : IEOSOnConnectLogin, IEOSSubManager
    {
        /// <summary>
        /// Contains a set of callbacks to be invoked whenever data pertaining
        /// to the service has been updated locally.
        /// </summary>
        protected ISet<Action> _listenerCallbacks = new HashSet<Action>();

        /// <summary>
        /// Indicates whether any listener callbacks have ever been invoked.
        /// </summary>
        protected bool _listenersHaveBeenNotified = false;

        /// <summary>
        /// Adds a given callback - to be invoked by the service whenever
        /// data pertaining to the service has been updated.
        /// </summary>
        /// <param name="callback">
        /// The callback to invoke when data pertaining to this manager has been
        /// updated locally.
        /// </param>
        /// <param name="initialInvoke">
        /// When adding the listener callback, if the service has previously
        /// notified other listeners, then invoke the provided callback
        /// immediately after adding it to the collection of listener callbacks.
        /// </param>
        public void AddListenerCallback(Action callback, bool initialInvoke = true)
        {
            _listenerCallbacks.Add(callback);

            // if the callback for the listener has been added after
            // notifications have been sent to other listeners, then execute the
            // callback right away
            if (_listenersHaveBeenNotified && initialInvoke)
            {
                callback.Invoke();
            }
        }

        /// <summary>
        /// Removes a specific callback from the list of callbacks to call when
        /// data for the service has been updated locally.
        /// </summary>
        /// <param name="callback">The callback to remove.</param>
        public void RemoveListenerCallback(Action callback)
        {
            _listenerCallbacks.Remove(callback);
        }

        /// <summary>
        /// Executes the callbacks for all the listeners that have registered
        /// callbacks with this service.
        /// </summary>
        protected void NotifyListeners()
        {
            foreach (Action callback in _listenerCallbacks)
            {
                callback?.Invoke();
            }

            // set the flag indicating that listeners have been notified, but
            // only if any of them were actually notified (and if the flag
            // hasn't already been set).
            if (!_listenersHaveBeenNotified && _listenerCallbacks.Count > 0)
            {
                _listenersHaveBeenNotified = true;
            }
        }

        /// <summary>
        /// Invoked when connect login callback has been executed.
        /// </summary>
        /// <param name="loginCallbackInfo">
        /// Information regarding the attempt to login.
        /// </param>
        public void OnConnectLogin(LoginCallbackInfo loginCallbackInfo)
        {
            if (loginCallbackInfo.ResultCode != Result.Success)
            {
                UnityEngine.Debug.LogWarning($"Connect login failed. Result code: \"{loginCallbackInfo.ResultCode}\".");
            }

            PostLogin(loginCallbackInfo.LocalUserId);
        }

        /// <summary>
        /// To be called after a login takes place.
        /// </summary>
        /// <param name="productUserId"></param>
        protected abstract void PostLogin(ProductUserId productUserId);

        /// <summary>
        /// Update the local data for the service.
        /// </summary>
        public void Refresh()
        {
            RefreshLocalData();
            NotifyListeners();
        }

        protected abstract void RefreshLocalData();
    }
}