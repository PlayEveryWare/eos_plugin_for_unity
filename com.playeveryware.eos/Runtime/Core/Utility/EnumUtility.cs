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

namespace PlayEveryWare.EpicOnlineServices.Extensions
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Provides support for alternate string parsing of enum values.
    /// </summary>
    internal static class EnumUtility<TEnum> where TEnum : struct, Enum
    {
        /// <summary>
        /// Tries to parse the given string into an enum value. If the string
        /// provided is either empty or null, the value will be set to the
        /// indicated defaultValue. If the string can be parsed successfully
        /// using the System.Enum.TryParse method, that result is returned. If
        /// that approach fails, a lookup is performed on the provided
        /// customMapping. If that fails, then the result is set to the
        /// specified default value, and false is returned.
        /// </summary>
        /// <param name="enumValueString">
        /// The string to parse into an enum value.
        /// </param>
        /// <param name="customMappings">
        /// A mapping of string values to enum types.
        /// </param>
        /// <param name="result">
        /// The result of parsing the given string into an enum value of the
        /// type specified.
        /// </param>
        /// <param name="defaultValue">
        /// The default enum value to use if the provided string is either null,
        /// empty, or cannot be parsed into a value.
        /// </param>
        /// <returns>
        /// True if the string was successfully parsed into an enum value, false
        /// otherwise.
        /// </returns>
        public static bool TryParse(
            string enumValueString, 
            IDictionary<string, TEnum> customMappings, 
            out TEnum result, 
            TEnum defaultValue = default)
        {
            result = defaultValue;

            // Allow for an empty or null string to indicate the explicit
            // default enum value indicated should be set.
            if (string.IsNullOrEmpty(enumValueString))
            {
                return true;
            }

            // Try to parse the string using the Enum.TryParse method, fall back
            // on the custom mapping dictionary declared above if that fails.
            if (Enum.TryParse(enumValueString, out result))
            {
                return true;
            }

            // Attempt to parse the enum using the provided custom mappings map.
            if (customMappings.TryGetValue(enumValueString, out result))
            {
                return true;
            }

            Debug.LogError($"\"{enumValueString}\" was not recognized as a valid {nameof(TEnum)} value, and parsing failed.");
            return false;

        }

        /// <summary>
        /// Tries to parse the given strings into an enum value. If the list of
        /// strings provided is either empty or null, the value will be set to
        /// the indicated defaultValue. If the strings can all be parsed
        /// successfully using the System.Enum.TryParse method, that result is
        /// returned. If that approach fails, a lookup is performed on the
        /// provided customMapping. If a single string value within the provided
        /// list cannot be successfully parsed into a value, then this function
        /// will return false, and the result will be set to the indicated
        /// default value.
        /// </summary>
        /// <param name="enumValuesAsStrings">
        /// The list of strings to parse into an enum value.
        /// </param>
        /// <param name="customMappings">
        /// A mapping of string values to enum types.
        /// </param>
        /// <param name="result">
        /// The result of parsing the given list of strings into an enum value
        /// of the type specified.
        /// </param>
        /// <param name="defaultValue">
        /// The default enum value to use if the provided list of strings is
        /// empty, or if a single string within the list cannot be successfully
        /// parsed into an enum value.
        /// </param>
        /// <returns>
        /// True if the list of strings were successfully parsed into an enum
        /// value, false otherwise.
        /// </returns>
        public static bool TryParse(
            IList<string> enumValuesAsStrings,
            IDictionary<string, TEnum> customMappings,
            out TEnum result,
            TEnum defaultValue = default)
        {
            result = defaultValue;

            foreach (string enumValueAsString in enumValuesAsStrings)
            {
                if (!TryParse(enumValueAsString, customMappings, out TEnum enumValue, defaultValue))
                {
                    // Set the result to defaultValue again, because it could be
                    // that previous iterations of this loop set it to some
                    // value other than the default.
                    result = defaultValue;
                    return false;
                }

                result = Combine(result, enumValue);
            }

            return true;
        }

        /// <summary>
        /// Combines the values of two enum types by getting their underlying
        /// data type and performing the bitwise OR operation on those values,
        /// and converting the resulting value back into an enum type.
        /// </summary>
        /// <param name="current">The current flag value.</param>
        /// <param name="toAdd">The flag to perform a bitwise OR with.</param>
        /// <returns>
        /// The result of a bitwise OR operation between the given enum values.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the underlying enum data type is not either int or ulong.
        /// </exception>
        private static TEnum Combine(TEnum current, TEnum toAdd)
        {
            Type underlyingType = Enum.GetUnderlyingType(typeof(TEnum));

            if (underlyingType == typeof(int))
            {
                int currentInt = Convert.ToInt32(current);
                int toAddInt = Convert.ToInt32(toAdd);
                int resultInt = currentInt | toAddInt;
                return (TEnum)Enum.ToObject(typeof(TEnum), resultInt);
            }
            
            if (underlyingType == typeof(uint))
            {
                uint currentUInt = Convert.ToUInt32(current);
                uint toAddUInt = Convert.ToUInt32(toAdd);
                uint resultUInt = currentUInt | toAddUInt;
                return (TEnum)Enum.ToObject(typeof(TEnum), resultUInt);
            }

            if (underlyingType == typeof(long))
            {
                long currentLong = Convert.ToInt64(current);
                long toAddLong = Convert.ToInt64(toAdd);
                long resultLong = currentLong | toAddLong;
                return (TEnum)Enum.ToObject(typeof(TEnum), resultLong);
            }

            if (underlyingType == typeof(ulong))
            {
                ulong currentULong = Convert.ToUInt64(current);
                ulong toAddULong = Convert.ToUInt64(toAdd);
                ulong resultULong = currentULong | toAddULong;
                return (TEnum)Enum.ToObject(typeof(TEnum), resultULong);
            }

            throw new ArgumentException($"Unsupported enum underlying type: \"{underlyingType.FullName}.\"");
        }

        /// <summary>
        /// Enum types that have the attribute [Flags], and have a compatible
        /// underlying type can be combined using bitwise operations. This
        /// utility function will take that combined value and decompose it into
        /// the individual Enum values represented by the value.
        ///
        /// This is mostly useful for debugging and for displaying the value in
        /// user interfaces, such as instances where the user can change the
        /// values.
        /// </summary>
        /// <param name="bitFlag">
        /// The possible result of combining enum values using bitwise
        /// operations.
        /// </param>
        /// <returns>
        /// The discrete values of which this combined flag is composed of.
        /// </returns>
        public static IEnumerable<TEnum> GetEnumerator(TEnum bitFlag)
        {
            IList<TEnum> compositeParts = new List<TEnum>();

            foreach (TEnum enumValue in Enum.GetValues(typeof(TEnum)))
            {
                if (bitFlag.HasFlag(enumValue))
                {
                    compositeParts.Add(enumValue);
                }
            }
            
            return compositeParts;
        }
    }
}