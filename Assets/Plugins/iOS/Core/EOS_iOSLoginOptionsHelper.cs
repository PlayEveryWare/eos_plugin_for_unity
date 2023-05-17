using System;
using Epic.OnlineServices.Auth;
using System.Runtime.InteropServices;

#if UNITY_IOS && !UNITY_EDITOR
public static class EOS_iOSLoginOptionsHelper
{
    //-------------------------------------------------------------------------
    /// <summary>
    /// Create App Controller necessary for iOS login options
    /// </summary>
    [DllImport("__Internal")]
    static private extern IntPtr LoginUtility_get_app_controller();

    //-------------------------------------------------------------------------
    /// <summary>
    /// Make Login Options for iOS Specific
    /// </summary>
    public static IOSLoginOptions MakeIOSLoginOptionsFromDefualt(Epic.OnlineServices.Auth.LoginOptions loginOptions)
    {
        IOSLoginOptions modifiedLoginOptions = new IOSLoginOptions();
        modifiedLoginOptions.ScopeFlags = loginOptions.ScopeFlags;

        var credentials = new IOSCredentials();

        credentials.Token = loginOptions.Credentials.Value.Token;
        credentials.Id = loginOptions.Credentials.Value.Id;
        credentials.Type = loginOptions.Credentials.Value.Type;
        credentials.ExternalType = loginOptions.Credentials.Value.ExternalType;

        var systemAuthCredentialsOptions = new IOSCredentialsSystemAuthCredentialsOptions();

        systemAuthCredentialsOptions.PresentationContextProviding = LoginUtility_get_app_controller();
        credentials.SystemAuthCredentialsOptions = systemAuthCredentialsOptions;

        modifiedLoginOptions.Credentials = credentials;

        return modifiedLoginOptions;
    }
}
#endif