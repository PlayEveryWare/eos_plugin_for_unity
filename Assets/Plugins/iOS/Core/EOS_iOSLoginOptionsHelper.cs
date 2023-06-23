/*
* Copyright (c) 2023 PlayEveryWare
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


#if !EOS_DISABLE
using System;
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

    public static Epic.OnlineServices.Auth.IOSLoginOptions MakeIOSLoginOptionsFromDefualt(Epic.OnlineServices.Auth.LoginOptions loginOptions)
    {
        Epic.OnlineServices.Auth.IOSLoginOptions modifiedLoginOptions = new Epic.OnlineServices.Auth.IOSLoginOptions();
        modifiedLoginOptions.ScopeFlags = loginOptions.ScopeFlags;

        var credentials = new Epic.OnlineServices.Auth.IOSCredentials();

        credentials.Token = loginOptions.Credentials.Value.Token;
        credentials.Id = loginOptions.Credentials.Value.Id;
        credentials.Type = loginOptions.Credentials.Value.Type;
        credentials.ExternalType = loginOptions.Credentials.Value.ExternalType;

        var systemAuthCredentialsOptions = new Epic.OnlineServices.Auth.IOSCredentialsSystemAuthCredentialsOptions();

        systemAuthCredentialsOptions.PresentationContextProviding = LoginUtility_get_app_controller();
        credentials.SystemAuthCredentialsOptions = systemAuthCredentialsOptions;

        modifiedLoginOptions.Credentials = credentials;

        return modifiedLoginOptions;
    }

}
#endif
#endif