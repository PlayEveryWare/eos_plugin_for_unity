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
namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using Epic.OnlineServices;
    using System;

    public static class ExternalCredentialTypeExtensions
    {
        public static bool IsDemoed(this ExternalCredentialType externalCredentialType)
        {
            switch (externalCredentialType)
            {
                // These credential types are demonstrated in the samples
                case ExternalCredentialType.GoogleIdToken:
                case ExternalCredentialType.SteamAppTicket:
                case ExternalCredentialType.DiscordAccessToken:
                case ExternalCredentialType.OpenidAccessToken:
                case ExternalCredentialType.DeviceidAccessToken:
                case ExternalCredentialType.AppleIdToken:
                case ExternalCredentialType.SteamSessionTicket:
                case ExternalCredentialType.OculusUseridNonce:
                    return true;
                // These credential types are not demonstrated in the samples
                case ExternalCredentialType.ItchioJwt:
                case ExternalCredentialType.ItchioKey:
                case ExternalCredentialType.EpicIdToken:
                case ExternalCredentialType.AmazonAccessToken:
                case ExternalCredentialType.GogSessionTicket:
                case ExternalCredentialType.NintendoIdToken:
                case ExternalCredentialType.NintendoNsaIdToken:
                case ExternalCredentialType.UplayAccessToken:
                case ExternalCredentialType.PsnIdToken:
                case ExternalCredentialType.XblXstsToken:
                case ExternalCredentialType.Epic:
                case ExternalCredentialType.ViveportUserToken:
                    return false;
                default:
                    // Note: This compile conditional is here so that in the
                    // editor an exception is thrown so the user can resolve the
                    // problem. If not in editor mode, then assume that the 
                    // credential given is not supported.
#if UNITY_EDITOR
                    throw new ArgumentOutOfRangeException(
                        nameof(externalCredentialType),
                        externalCredentialType, 
                        $"ExternalCredentialType \"{Enum.GetName(typeof(ExternalCredentialType), externalCredentialType)}\" is not recognized.");
#else
                    return false;
#endif
            }
        }
    }
}