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
 * Author: Paul Hazen (paul.hazen@playeveryware.com)
 *
 * Date:   2024-06-17
 *
 * Notes: This file contains implementation of functionality that is common to
 *        many of the different managers. Not all managers make use of it - as
 *        transitioning will be done gradually and as areas are refactored.
 *        Consequently, only EOSAchievementManager and StatsManager make use of
 *        this base class.
 *
 */

//#define ENABLE_DEBUG_EOSSTATSMANAGER

namespace PlayEveryWare.EpicOnlineServices
{
    using System;
    using System.Collections.Generic;

    using Epic.OnlineServices;
    using Epic.OnlineServices.Connect;
    using System.Threading.Tasks;

    /// <summary>
    /// Contains implementation of common functionality between different
    /// EOS Service managers (Currently EOSAchievementManager and StatsManager).
    /// </summary>
    public abstract class ServiceManager : IEOSOnConnectLogin, IEOSSubManager
    {
        /// <summary>
        /// Stores a list of functions to be called whenever data related to
        /// the manager has been updated.
        /// </summary>
        protected IList<Action> _updateCallbacks = new List<Action>();

        /// <summary>
        /// Called when connect login has taken place.
        /// </summary>
        /// <param name="loginCallbackInfo"></param>
        public void OnConnectLogin(LoginCallbackInfo loginCallbackInfo)
        {
            if (loginCallbackInfo.ResultCode != Result.Success)
            {
                UnityEngine.Debug.Log($"Player login did not succeed. Result code: {Enum.GetName(typeof(Result), loginCallbackInfo.ResultCode)}");
                return;
            }
            OnPlayerLogin(loginCallbackInfo.LocalUserId);
        }

        /// <summary>
        /// Should be called when a player has been successfully logged in.
        /// </summary>
        /// <param name="productUserId">
        /// The ProductUserId of the player that has just been logged in.
        /// </param>
        protected abstract void OnPlayerLogin(ProductUserId productUserId);

        /// <summary>
        /// Implementation of this function should refresh the locally cached
        /// data managed by the service manager in question.
        /// </summary>
        public abstract Task RefreshAsync();

        /// <summary>
        /// Trigger the notification callbacks that have been registered,
        /// letting any consumers know that the data base been updated.
        /// </summary>
        protected void NotifyUpdated()
        {
            // Make copy of the update callbacks so that the callback cannot
            // inadvertently remove from or add to the list, which would break
            // the foreach loop.
            IList<Action> updateCallBacksCopy = new List<Action>(_updateCallbacks);
            foreach (Action action in updateCallBacksCopy)
                action();
        }

        /// <summary>
        /// Adds a given callback - to be invoked by the manager whenever
        /// data pertaining to the manager has been updated.
        /// </summary>
        /// <param name="callback">
        /// The callback to invoke when data pertaining to this manager has been
        /// updated locally.
        /// </param>
        public void AddUpdateCallback(Action callback)
        {
            _updateCallbacks.Add(callback);
        }

        /// <summary>
        /// Removes a specific callback from the list of callbacks to call when
        /// data for achievements and stats has been updated locally.
        /// </summary>
        /// <param name="callback">The callback to remove.</param>
        public void RemoveUpdateCallback(Action callback)
        {
            _updateCallbacks.Remove(callback);
        }
    }
}