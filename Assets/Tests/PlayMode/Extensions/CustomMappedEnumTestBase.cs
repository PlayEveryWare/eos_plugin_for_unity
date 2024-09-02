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

namespace PlayEveryWare.EpicOnlineServices.Tests.Extensions
{
    using NUnit.Framework;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CustomMappedEnumTestBase<TEnum> where TEnum : struct, Enum
    {
        /// <summary>
        /// Helper function that retrieves an IEnumerable of all the values
        /// defined within the indicated enum.
        /// </summary>
        /// <returns>
        /// An enumerable that allows traversal of the values in the indicated
        /// enum.
        /// </returns>
        private static IEnumerable<TEnum> GetEnumValues()
        {
            return typeof(TEnum).GetEnumValues().Cast<TEnum>();
        }

        /// <summary>
        /// Checks to make sure that all values for the enum have a description
        /// defined by the given function that gets the description.
        /// </summary>
        /// <param name="getDescriptionFn">
        /// The function used to retrieve the description for the enumeration.
        /// Typically, this is a function defined in an extension class for the
        /// specific enum being tested.
        /// </param>
        protected static void AllValues_HaveDescription(
            Func<TEnum, string> getDescriptionFn)
        {
            foreach (TEnum enumValue in GetEnumValues())
            {
                // Run the get description function to guarantee that it does 
                // not throw an exception when called (if a custom mapped enum
                // does not have a defined description, it should throw an
                // exception because it can signal to the developer that the 
                // custom mapping is out of date with the definition of the enum
                // it is mapped against.
                Assert.DoesNotThrow(() => getDescriptionFn(enumValue), 
                    $"The extension class for the enum {typeof(TEnum).Name} " +
                    $"does not have a switch clause providing a description " +
                    $"text for the enum value \"{enumValue}\".");
            }
        }

        /// <summary>
        /// Checks to make sure that there is a custom mapping for each value in
        /// the indicated enum. 
        /// </summary>
        /// <param name="customMappings">
        /// The dictionary that provides mapping from custom string values to
        /// specific values within the enum.
        /// </param>
        protected static void CustomMappings_Exist(
            Dictionary<string, TEnum> customMappings)
        {
            foreach (TEnum enumValue in GetEnumValues())
            {
                Assert.IsTrue(customMappings.ContainsValue(enumValue),
                    $"The extension class for the enum {typeof(TEnum).Name} " +
                    $"does not have a custom mapping defined for the enum " +
                    $"value \"{enumValue}\".");
            }
        }
    }
}
