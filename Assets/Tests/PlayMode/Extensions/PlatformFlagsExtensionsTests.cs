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
    using Epic.OnlineServices.Platform;
    using EpicOnlineServices.Extensions;

    public class PlatformFlagsExtensionsTests : CustomMappedEnumTestBase<PlatformFlags>
    { 
        /// <summary>
        /// Tests to make sure that there is a description defined for each of
        /// the enum values.
        /// </summary>
        [Test]
        public static void AllValues_HaveDescription()
        {
            AllValues_HaveDescription(enumValue => enumValue.GetDescription());
        }

        /// <summary>
        /// Guarantees that there is a custom mapping entry for each of the enum
        /// values.
        /// </summary>
        [Test]
        public static void CustomMappings_Exists()
        {
            CustomMappings_Exist(PlatformFlagsExtensions.CustomMappings);
        }

        /// <summary>
        /// Guarantees that the TryParse method works for all the valid strings
        /// that can be parsed, and that parsing of an invalid string is handled
        /// correctly.
        /// </summary>
        [Test]
        public static void Test_TryParse()
        {
            Test_TryParse(
                PlatformFlagsExtensions.CustomMappings,
                PlatformFlagsExtensions.TryParse
            );
        }
    }
}
