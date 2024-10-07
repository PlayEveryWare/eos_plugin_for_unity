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

#if !EOS_DISABLE

namespace PlayEveryWare.EpicOnlineServices
{
    using Epic.OnlineServices;
    using Epic.OnlineServices.Auth;
    using System;
    using UnityEngine;

    /// <summary>
    /// Used to listen for authentication events from EOSManager.
    /// </summary>
    public class AuthenticationListener: IAuthInterfaceEventListener, IConnectInterfaceEventListener, IDisposable
    {
        /// <summary>
        /// Identifies the kind of authentication change.
        /// </summary>
        public enum LoginChangeKind
        {
            /// <summary>
            /// Represents a login change relating to the Auth-login type with EOS.
            /// A user logged in with the Auth Interface has access to Epic
            /// Account Services (EAS) operations.
            /// </summary>
            Auth,

            /// <summary>
            /// Represents a login change relating to the Connect-login type with EOS.
            /// A user logged in with the Connect Interface has access to all
            /// EOS Game Services.
            /// Typically a user will be logged in to Auth and then afterwards
            /// logged in to Connect.
            /// </summary>
            Connect
        }

        /// <summary>
        /// Used to describe functions that handle change in authentication
        /// state.
        /// </summary>
        /// <param name="authenticated">
        /// True if the authentication state has changed to authenticated, False
        /// otherwise.
        /// </param>
        public delegate void AuthenticationChangedEventHandler(bool authenticated, LoginChangeKind changeType);

        /// <summary>
        /// Event that triggers when the state of authentication has changed.
        /// </summary>
        public event AuthenticationChangedEventHandler AuthenticationChanged;

        #region Singleton Pattern Implementation

        /// <summary>
        /// Lazy instance for singleton allows for thread-safe interactions with
        /// the AuthenticationListener
        /// </summary>
        private static readonly Lazy<AuthenticationListener> s_LazyInstance = new(() => new AuthenticationListener());

        /// <summary>
        /// Accessor for the instance.
        /// </summary>
        public static AuthenticationListener Instance
        {
            get
            {
                return s_LazyInstance.Value;
            }
        }

        /// <summary>
        /// Private constructor ensures enforcement of the singleton pattern.
        /// </summary>
        private AuthenticationListener() 
        {
            EOSManager.Instance.AddAuthLoginListener(this);
            EOSManager.Instance.AddAuthLogoutListener(this);
            EOSManager.Instance.AddConnectLoginListener(this);
        }
        
        #endregion

        private bool? _isAuthenticated;

        /// <summary>
        /// Indicates whether the authentication listener has received an
        /// AuthenticationChanged event that indicates a user has been logged
        /// in.
        /// </summary>
        public bool IsAuthenticated
        {
            get
            {
                // if _isAuthenticated has no value, then we do not know if
                // we are authenticated or not, so we should say that we are not
                return _isAuthenticated.HasValue && _isAuthenticated.Value;
            }
        }

        /// <summary>
        /// Based on the given result code, determine if the authentication
        /// state change event needs to be triggered or not, and if so trigger
        /// it with the appropriate event args.
        /// </summary>
        /// <param name="attemptedState"></param>
        /// <param name="attemptResult"></param>
        /// <param name="changeType">The type of authentication change.</param>
        private void TriggerAuthenticationChangedEvent(bool attemptedState, Result attemptResult, LoginChangeKind changeType)
        {
            // If the attempt to change the state of authentication did not 
            // succeed, then log a warning and stop.
            if (attemptResult != Result.Success)
            {
                Debug.LogWarning($"Authentication change attempt failed with following result code: {attemptResult}");
                return;
            }

            // Keep track of whether the user is authenticated.
            _isAuthenticated = attemptedState;

            // Trigger the event indicating that the state of authentication for 
            // the user has changed.
            AuthenticationChanged?.Invoke(attemptedState, changeType);
        }

        /// <summary>
        /// Called by EOSManager when auth login has taken place.
        /// </summary>
        /// <param name="loginCallbackInfo">
        /// Callback parameters for the authentication operation.
        /// </param>
        public void OnAuthLogin(LoginCallbackInfo loginCallbackInfo)
        {
            TriggerAuthenticationChangedEvent(true, loginCallbackInfo.ResultCode, LoginChangeKind.Auth);
        }

        /// <summary>
        /// Called by EOSManager when auth logout has taken place.
        /// </summary>
        /// <param name="logoutCallbackInfo">
        /// Callback parameters for the logout operation.
        /// </param>
        public void OnAuthLogout(LogoutCallbackInfo logoutCallbackInfo)
        {
            TriggerAuthenticationChangedEvent(false, logoutCallbackInfo.ResultCode, LoginChangeKind.Auth);
        }

        /// <summary>
        /// Called by EOSManager when connect login has taken place.
        /// </summary>
        /// <param name="loginCallbackInfo">
        /// Callback parameters for the connect login operation.
        /// </param>
        public void OnConnectLogin(Epic.OnlineServices.Connect.LoginCallbackInfo loginCallbackInfo)
        {
            TriggerAuthenticationChangedEvent(true, loginCallbackInfo.ResultCode, LoginChangeKind.Connect);
        }

        /// <summary>
        /// Dispose of the AuthenticationListener, removing it as a listener
        /// from the various ways that EOSManager keeps track of it.
        /// </summary>
        public void Dispose()
        {
            EOSManager.Instance.RemoveAuthLoginListener(this);
            EOSManager.Instance.RemoveAuthLogoutListener(this);
            EOSManager.Instance.RemoveConnectLoginListener(this);
        }
    }
}

#endif