namespace PlayEveryWare.EpicOnlineServices
{
    using Epic.OnlineServices.Auth;
    using Epic.OnlineServices.Lobby;
    using System;
    using UnityEngine;

    public abstract class AuthenticationListener : MonoBehaviour, IAuthInterfaceEventListener, IConnectInterfaceEventListener, IDisposable
    {
        protected AuthenticationListener()
        {
            EOSManager.Instance.AddAuthLoginListener(this);
            EOSManager.Instance.AddAuthLogoutListener(this);
            EOSManager.Instance.AddConnectLoginListener(this);
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

        public void OnDestroy()
        {
            Debug.Log($"Destroying AuthenticationListener.");
            Dispose();
        }

        public void Dispose()
        {
            EOSManager.Instance.RemoveAuthLoginListener(this);
            EOSManager.Instance.RemoveAuthLogoutListener(this);
            EOSManager.Instance.RemoveConnectLoginListener(this);
        }
    }
}