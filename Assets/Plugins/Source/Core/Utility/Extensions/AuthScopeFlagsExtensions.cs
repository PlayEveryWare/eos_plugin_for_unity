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
    using System.Collections.Generic;

    /// <summary>
    /// Provides a means of parsing string representations of enum values for
    /// the enum type AuthScopeFlags.
    /// </summary>
    public static class AuthScopeFlagsExtensions
    {
        /// <summary>
        /// An alternate set of string values that can be parsed into
        /// AuthScopeFlags enum values.
        /// </summary>
        private static readonly Dictionary<string, AuthScopeFlags> s_customMappings = new()
        {
            {"EOS_AS_NoFlags",           AuthScopeFlags.NoFlags},
            {"EOS_AS_BasicProfile",      AuthScopeFlags.BasicProfile},
            {"EOS_AS_FriendsList",       AuthScopeFlags.FriendsList},
            {"EOS_AS_Presence",          AuthScopeFlags.Presence},
            {"EOS_AS_FriendsManagement", AuthScopeFlags.FriendsManagement},
            {"EOS_AS_Email",             AuthScopeFlags.Email},
            {"EOS_AS_Country",           AuthScopeFlags.Country}
        };

        /// <summary>
        /// Tries to parse the given string into an enum value.
        /// </summary>
        /// <param name="str">The string to parse into an enum value.</param>
        /// <param name="flags">
        /// The enum value resulting from the parse operation.
        /// </param>
        /// <returns>True if parsing was successful, false otherwise.</returns>
        public static bool TryParse(string str, out AuthScopeFlags flags)
        {
            return EnumUtility<AuthScopeFlags>.TryParse(str, s_customMappings, out flags);
        }

        public static bool TryParse(IList<string> stringFlags, out AuthScopeFlags flags)
        {
            return EnumUtility<AuthScopeFlags>.TryParse(stringFlags, s_customMappings, out flags);
        }
    }
}

#endif