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
    using Epic.OnlineServices.Platform;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides methods to assist in the interaction of PlatformFlag
    /// operations.
    /// </summary>
    public static class PlatformFlagsExtensions
    {
        /// <summary>
        /// Contains a mapping of the alternative string values for each of the
        /// PlatformFlag enum values.
        /// </summary>
        private static readonly Dictionary<string, PlatformFlags> s_customMappings =
            new()
            {
                {"EOS_PF_NONE",                          PlatformFlags.None},
                {"EOS_PF_LOADING_IN_EDITOR",             PlatformFlags.LoadingInEditor},
                {"EOS_PF_DISABLE_OVERLAY",               PlatformFlags.DisableOverlay},
                {"EOS_PF_DISABLE_SOCIAL_OVERLAY",        PlatformFlags.DisableOverlay},
                {"EOS_PF_WINDOWS_ENABLE_OVERLAY_D3D9",   PlatformFlags.WindowsEnableOverlayD3D9},
                {"EOS_PF_WINDOWS_ENABLE_OVERLAY_D3D10",  PlatformFlags.WindowsEnableOverlayD3D10},
                {"EOS_PF_WINDOWS_ENABLE_OVERLAY_OPENGL", PlatformFlags.WindowsEnableOverlayOpengl},
                {"EOS_PF_RESERVED1",                     PlatformFlags.Reserved1},
            };

        /// <summary>
        /// Tries to parse the given list of strings representing individual
        /// PlatformFlags enum values, and performing a
        /// bitwise OR operation on those values.
        /// </summary>
        /// <param name="stringFlags">
        /// List of strings representing individual PlatformFlags enum values.
        /// </param>
        /// <param name="result">
        /// The result of performing a bitwise OR operation on the values that
        /// are represented by the list of string values.
        /// </param>
        /// <returns>
        /// True if all the list of strings was successfully parsed, and the
        /// resulting list of enum values was bitwise ORed together.
        /// </returns>
        public static bool TryParse(IList<string> stringFlags, out PlatformFlags result)
        {
            return EnumUtility<PlatformFlags>.TryParse(stringFlags, s_customMappings, out result);
        }
    }
}
#endif