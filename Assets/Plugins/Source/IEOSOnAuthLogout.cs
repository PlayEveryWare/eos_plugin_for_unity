namespace PlayEveryWare.EpicOnlineServices
{
    public interface IEOSOnAuthLogout : IEOSSubManager
    {
        void OnAuthLogout(Epic.OnlineServices.Auth.LogoutCallbackInfo logoutCallbackInfo);
    }
}
