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
        /// InputStateButtonFlags enum values, and performing a
        /// bitwise OR operation on those values.
        /// </summary>
        /// <param name="stringFlags">
        /// List of strings representing individual InputStateButtonFlags enum values.
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

        /// <summary>
        /// Parses an InputStateButtonFlags value in to its component strings.
        /// These strings can then be safely stored in a configuration value.
        /// </summary>
        /// <param name="flags">Enum to separate into parts.</param>
        /// <returns>A list of strings representing these flags.</returns>
        public static List<string> FlagsToStrings(this InputStateButtonFlags flags)
        {
            List<string> flagStrings = new List<string>();

            // Iterate over all possible flag values
            foreach (InputStateButtonFlags flagOption in Enum.GetValues(typeof(InputStateButtonFlags)))
            {
                // If the passed in Enum doesn't have this flag, skip it
                if (!flags.HasFlag(flagOption))
                {
                    continue;
                }

                // Look up this flag's name in the custom mappings dictionary
                // The dictionary is keyed by string value and valued by enum,
                // so it must be entirely traversed to find its matching string
                bool foundMapping = false;
                foreach (KeyValuePair<string, InputStateButtonFlags> mapping in CustomMappings)
                {
                    // Continue if this isn't the flag that matches the current option
                    if (mapping.Value != flagOption)
                    {
                        continue;
                    }

                    // This is the value needed, so use it
                    flagStrings.Add(mapping.Key);
                    foundMapping = true;
                    continue;
                }

                // If no custom mapping was found, use ToString instead
                if (!foundMapping)
                {
                    flagStrings.Add(flagOption.ToString());
                }
            }

            return flagStrings;
        }
    }
}

#endif