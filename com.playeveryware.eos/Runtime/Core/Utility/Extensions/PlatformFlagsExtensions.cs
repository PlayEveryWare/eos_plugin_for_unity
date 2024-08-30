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
        public static Dictionary<string, PlatformFlags> CustomMappings { get; } =
            new()
            {
                {"EOS_PF_NONE",                                       PlatformFlags.None},
                {"EOS_PF_LOADING_IN_EDITOR",                          PlatformFlags.LoadingInEditor},
                {"EOS_PF_DISABLE_OVERLAY",                            PlatformFlags.DisableOverlay},
                {"EOS_PF_DISABLE_SOCIAL_OVERLAY",                     PlatformFlags.DisableSocialOverlay},
                {"EOS_PF_WINDOWS_ENABLE_OVERLAY_D3D9",                PlatformFlags.WindowsEnableOverlayD3D9},
                {"EOS_PF_WINDOWS_ENABLE_OVERLAY_D3D10",               PlatformFlags.WindowsEnableOverlayD3D10},
                {"EOS_PF_WINDOWS_ENABLE_OVERLAY_OPENGL",              PlatformFlags.WindowsEnableOverlayOpengl},
                {"EOS_PF_CONSOLE_ENABLE_OVERLAY_AUTOMATIC_UNLOADING", PlatformFlags.ConsoleEnableOverlayAutomaticUnloading},
                {"EOS_PF_RESERVED1",                                  PlatformFlags.Reserved1},
            };

        public static string GetDescription(this PlatformFlags platformFlags)
        {
            return platformFlags switch
            {
                PlatformFlags.None => "No flags.",
                PlatformFlags.LoadingInEditor => "A bit that indicates the SDK is being loaded in a game editor, like Unity or UE4 Play-in-Editor",
                PlatformFlags.DisableOverlay => "A bit that indicates the SDK should skip initialization of the overlay, which is used by the in-app purchase flow and social overlay. This bit is implied by LoadingInEditor.",
                PlatformFlags.DisableSocialOverlay => "A bit that indicates the SDK should skip initialization of the social overlay, which provides an overlay UI for social features. This bit is implied by LoadingInEditor or DisableOverlay.",
                PlatformFlags.Reserved1 => "A reserved bit.",
                PlatformFlags.WindowsEnableOverlayD3D9 => "A bit that indicates your game would like to opt-in to experimental Direct3D 9 support for the overlay. This flag is only relevant on Windows.",
                PlatformFlags.WindowsEnableOverlayD3D10 => "A bit that indicates your game would like to opt-in to experimental Direct3D 10 support for the overlay. This flag is only relevant on Windows.",
                PlatformFlags.WindowsEnableOverlayOpengl => "A bit that indicates your game would like to opt-in to experimental OpenGL support for the overlay. This flag is only relevant on Windows.",
                PlatformFlags.ConsoleEnableOverlayAutomaticUnloading => "A bit that indicates your game would like to opt-in to automatic unloading of the overlay module when possible. This flag is only relevant on Consoles.",
                _ => throw new ArgumentOutOfRangeException(nameof(platformFlags), platformFlags, null),
            };
        }

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
            return EnumUtility<PlatformFlags>.TryParse(stringFlags, CustomMappings, out result);
        }
    }
}
#endif