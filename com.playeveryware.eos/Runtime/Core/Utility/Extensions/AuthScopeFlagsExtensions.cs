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

namespace PlayEveryWare.EpicOnlineServices.Extensions
{
    using Epic.OnlineServices.Auth;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides a means of parsing string representations of enum values for
    /// the enum type AuthScopeFlags.
    /// </summary>
    public static class AuthScopeFlagsExtensions
    {
        /// <summary>
        /// Returns the internal custom mappings.
        /// </summary>
        public static Dictionary<string, AuthScopeFlags> CustomMappings { get; } = new()
        {
            {"EOS_AS_NoFlags",           AuthScopeFlags.NoFlags},
            {"EOS_AS_BasicProfile",      AuthScopeFlags.BasicProfile},
            {"EOS_AS_FriendsList",       AuthScopeFlags.FriendsList},
            {"EOS_AS_Presence",          AuthScopeFlags.Presence},
            {"EOS_AS_FriendsManagement", AuthScopeFlags.FriendsManagement},
            {"EOS_AS_Email",             AuthScopeFlags.Email},
            {"EOS_AS_Country",           AuthScopeFlags.Country}
        };

        public static string GetDescription(this AuthScopeFlags flags)
        {
            return flags switch
            {
                AuthScopeFlags.NoFlags => "No flags.",
                AuthScopeFlags.BasicProfile => "Permissions to see your account ID, display name, and language.",
                AuthScopeFlags.FriendsList => "Permissions to see a list of your friends who use this application.",
                AuthScopeFlags.Presence => "Permissions to set your online presence and see presence of your friends.",
                AuthScopeFlags.FriendsManagement => "Permissions to manage the Epic friends list. This cope is restricted to Epic first party products, and attempting to use it will result in authentication failures.",
                AuthScopeFlags.Email => "Permissions to see email in the response when fetching information for a user. This scope is restricted to Epic first party products, and attempting to use it will result in authentication failures.",
                AuthScopeFlags.Country => "Permissions to see your country.",
                _ => throw new ArgumentOutOfRangeException(nameof(flags), flags, null),
            };
        }

        /// <summary>
        /// Tries to parse the given list of strings representing individual
        /// AuthScopeFlags enum values, and performing a
        /// bitwise OR operation on those values.
        /// </summary>
        /// <param name="stringFlags">
        /// List of strings representing individual AuthScopeFlags enum values.
        /// </param>
        /// <param name="result">
        /// The result of performing a bitwise OR operation on the values that
        /// are represented by the list of string values.
        /// </param>
        /// <returns>
        /// True if all the list of strings was successfully parsed, and the
        /// resulting list of enum values was bitwise ORed together.
        /// </returns>
        public static bool TryParse(IList<string> stringFlags, out AuthScopeFlags result)
        {
            return EnumUtility<AuthScopeFlags>.TryParse(stringFlags, CustomMappings, out result);
        }
    }
}

#endif