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
    using Utility;
    using System;
    using System.Collections.Generic;
    using Epic.OnlineServices.UI;

    /// <summary>
    /// Provides a means of parsing string representations of enum values for
    /// the enum type InputStateButtonFlags.
    /// </summary>
    public static class InputStateButtonFlagsExtensions
    {
        /// <summary>
        /// Returns the internal custom mappings.
        /// </summary>
        public static Dictionary<string, InputStateButtonFlags> CustomMappings { get; } = new()
        {
            { "DPadDown", InputStateButtonFlags.DPadDown },
            { "DPadLeft", InputStateButtonFlags.DPadLeft },
            { "DPadRight", InputStateButtonFlags.DPadRight },
            { "DPadUp", InputStateButtonFlags.DPadUp },
            { "FaceButtonBottom", InputStateButtonFlags.FaceButtonBottom },
            { "FaceButtonLeft", InputStateButtonFlags.FaceButtonLeft },
            { "FaceButtonRight", InputStateButtonFlags.FaceButtonRight },
            { "FaceButtonTop", InputStateButtonFlags.FaceButtonTop },
            { "LeftShoulder", InputStateButtonFlags.LeftShoulder },
            { "LeftThumbstick", InputStateButtonFlags.LeftThumbstick },
            { "LeftTrigger", InputStateButtonFlags.LeftTrigger },
            { "RightShoulder", InputStateButtonFlags.RightShoulder },
            { "RightThumbstick", InputStateButtonFlags.RightThumbstick },
            { "RightTrigger", InputStateButtonFlags.RightTrigger },
            { "SpecialLeft", InputStateButtonFlags.SpecialLeft },
            { "SpecialRight", InputStateButtonFlags.SpecialRight }
        };

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
        public static bool TryParse(IList<string> stringFlags, out InputStateButtonFlags result)
        {
            return EnumUtility<InputStateButtonFlags>.TryParse(stringFlags, CustomMappings, out result);
        }
    }
}

#endif