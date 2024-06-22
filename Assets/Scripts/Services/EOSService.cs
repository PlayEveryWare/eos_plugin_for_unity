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
 */

namespace PlayEveryWare.EpicOnlineServices
{
    using System;
    using System.Collections.Generic;

    using Epic.OnlineServices;
    using Epic.OnlineServices.Connect;
    using System.Threading.Tasks;

    /// <summary>
    /// Contains implementation of common functionality between different
    /// EOS Service managers (Currently AchievementsService and StatsService).
    /// </summary>
    public abstract class EOSService : IConnectInterfaceEventListener, IEOSSubManager, IDisposable
    {
        /// <summary>
        /// Stores a list of functions to be called whenever data related to
        /// the manager has been updated.
        /// </summary>
        protected IList<Action> _updateCallbacks = new List<Action>();

        /// <summary>
        /// Indicates whether the service needs to have a user authenticated in
        /// order to function properly.
        /// </summary>
        private bool _requiresLoggedInWithConnectInterface;

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
        /// Base constructor for Service managers.
        /// </summary>
        /// <param name="requiresLoggedInWithConnectInterface">
        /// Indicates whether the service manager requires a user to be
        /// authenticated with the Connect Interface in order to function
        /// properly.
        /// </param>
        protected EOSService(bool requiresLoggedInWithConnectInterface)
        {
            _requiresLoggedInWithConnectInterface = requiresLoggedInWithConnectInterface;

            EOSManager.Instance.AddConnectLoginListener(this);

            _ = RefreshAsync();
        }

        /// <summary>
        /// Determines whether the user is currently authenticated using the
        /// Connect Interface provided by the EOS SDK.
        /// </summary>
        /// <returns>
        /// True if the user is authenticated using the Connect Interface, false
        /// otherwise.
        /// </returns>
        private static bool IsLoggedInWithConnectInterface()
        {
            ProductUserId userId = EOSManager.Instance.GetProductUserId();
            if (null == userId || false == userId.IsValid())
            {
                return false;
            }

            return EOSManager.Instance.GetEOSConnectInterface().GetLoginStatus(userId) == LoginStatus.LoggedIn;
        }

        /// <summary>
        /// Dispose function makes sure that the manager is removed from the
        /// collection of connect login listeners.
        /// </summary>
        public void Dispose()
        {
            EOSManager.Instance.RemoveConnectLoginListener(this);
        }

        /// <summary>
        /// Should be called when a player has been successfully logged in.
        /// </summary>
        /// <param name="productUserId">
        /// The ProductUserId of the player that has just been logged in.
        /// </param>
        protected abstract void OnPlayerLogin(ProductUserId productUserId);

        /// <summary>
        /// Refreshes the service manager
        /// </summary>
        /// <returns></returns>
        public async Task RefreshAsync()
        {
            // Check to see if authentication is required, if it's not then 
            // continue. If it is, then make sure a user is authenticated before
            // refreshing.
            if (!_requiresLoggedInWithConnectInterface || (_requiresLoggedInWithConnectInterface && IsLoggedInWithConnectInterface()))
            {
                await InternalRefreshAsync();
            }
        }
        
        /// <summary>
        /// Implementation of this function should refresh the locally cached
        /// data managed by the service manager in question.
        /// </summary>
        protected abstract Task InternalRefreshAsync();

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