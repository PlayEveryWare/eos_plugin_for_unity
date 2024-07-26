using PlayEveryWare.EpicOnlineServices.Samples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticationExpirationTestManager : MonoBehaviour
{
    public enum AuthenticationProviderToTest
    {
        Unset = 0,
        Steam = 1,
        Discord = 2,
        OpenID = 3
    }

    public UIDebugLog Log;

    public void SetAuthenticationProviderAndLogin(int toSet)
    {
        SetAuthenticationProviderAndLogin((AuthenticationProviderToTest)toSet);
    }

    public void SetAuthenticationProviderAndLogin(AuthenticationProviderToTest toSet)
    {
        switch (toSet)
        {
            case AuthenticationProviderToTest.Steam:
                TestSteam();
                break;
            case AuthenticationProviderToTest.Discord:
                TestSteam();
                break;
            case AuthenticationProviderToTest.OpenID:
                TestOpenID();
                break;
        }
    }

    void TestSteam()
    {

    }

    void TestDiscord()
    {

    }

    void TestOpenID()
    {

    }
}
