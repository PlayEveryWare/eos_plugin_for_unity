namespace PlayEveryWare.EpicOnlineServices
{
    public interface IEOSOnAuthLogin : IEOSSubManager
    {
        void OnAuthLogin(Epic.OnlineServices.Auth.LoginCallbackInfo loginCallbackInfo);
    }
}
