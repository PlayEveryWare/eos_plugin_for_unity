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
 *
 */

#if !EOS_DISABLE

namespace PlayEveryWare.EpicOnlineServices
{
    using System;

    /// <summary>
    /// This enum is a 1:1 "duplicate" of the PlatformFlags enum provided by the
    /// EOS SDK. The reason it is implemented here is due to the restricted
    /// nature of Unity's support for the underlying data type for enum classes
    /// in C#. Unity has some interesting edge cases in this regard, one of
    /// which is that various editor utilities cannot be utilized for enums
    /// whose underlying type is of an unsigned number type.
    /// </summary>
    [System.Flags]
    public enum WrappedPlatformFlags : int
    {
        None = 0x0,

        /// <summary>
        /// A bit that indicates the SDK is being loaded in a game editor, like Unity or UE4 Play-in-Editor
        /// </summary>
        LoadingInEditor = 0x00001,

        /// <summary>
        /// A bit that indicates the SDK should skip initialization of the overlay, which is used by the in-app purchase flow and social overlay. This bit is implied by <see cref="LoadingInEditor" />
        /// </summary>
        DisableOverlay = 0x00002,

        /// <summary>
        /// A bit that indicates the SDK should skip initialization of the social overlay, which provides an overlay UI for social features. This bit is implied by <see cref="LoadingInEditor" /> or <see cref="DisableOverlay" />
        /// </summary>
        DisableSocialOverlay = 0x00004,

        /// <summary>
        /// A reserved bit
        /// </summary>
        Reserved1 = 0x00008,

        /// <summary>
        /// A bit that indicates your game would like to opt-in to experimental Direct3D 9 support for the overlay. This flag is only relevant on Windows
        /// </summary>
        WindowsEnableOverlayD3D9 = 0x00010,

        /// <summary>
        /// A bit that indicates your game would like to opt-in to experimental Direct3D 10 support for the overlay. This flag is only relevant on Windows
        /// </summary>
        WindowsEnableOverlayD3D10 = 0x00020,

        /// <summary>
        /// A bit that indicates your game would like to opt-in to experimental OpenGL support for the overlay. This flag is only relevant on Windows
        /// </summary>
        WindowsEnableOverlayOpengl = 0x00040,

        /// <summary>
        /// A bit that indicates your game would like to opt-in to automatic unloading of the overlay module when possible. This flag is only relevant on Consoles
        /// </summary>
        ConsoleEnableOverlayAutomaticUnloading = 0x00080
    }

    public static class WrappedPlatformFlagsExtensions
    {
        public static bool IsSupported(this WrappedPlatformFlags platformFlags, PlatformManager.Platform platform)
        {
            switch (platformFlags)
            {
                case WrappedPlatformFlags.None:
                case WrappedPlatformFlags.LoadingInEditor:
                case WrappedPlatformFlags.DisableOverlay:
                case WrappedPlatformFlags.DisableSocialOverlay:
                case WrappedPlatformFlags.Reserved1:
                    return true;
                case WrappedPlatformFlags.WindowsEnableOverlayD3D9:
                case WrappedPlatformFlags.WindowsEnableOverlayD3D10:
                case WrappedPlatformFlags.WindowsEnableOverlayOpengl:
                    if (platform == PlatformManager.Platform.Windows)
                    {
                        return true;
                    }
                    break;
                case WrappedPlatformFlags.ConsoleEnableOverlayAutomaticUnloading:
                    if (platform == PlatformManager.Platform.Console)
                    {
                        return true;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(platformFlags), platformFlags, null);
            }

            return false;
        }
    }
}

#endif