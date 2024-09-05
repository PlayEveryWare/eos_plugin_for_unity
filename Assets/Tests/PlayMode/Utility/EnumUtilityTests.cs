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

namespace PlayEveryWare.EpicOnlineServices.Tests.Utility
{
    using EpicOnlineServices.Utility;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [TestFixture]
    public class EnumUtilityTests
    {
        [Flags]
        internal enum TestEnum
        {
            None = 0,
            OptionA = 1,
            OptionB = 2,
            OptionC = 4,
            Combined = OptionA | OptionB
        }

        [Test]
        public void TryParse_ValidEnumString_ReturnsTrueAndParsesValue()
        {
            const string input = "OptionA";
            const TestEnum expected = TestEnum.OptionA;

            TestTryParse(
                input,
                new Dictionary<string, TestEnum>() { { input, expected } },
                true,
                "The method should return true for a valid enum string.",
                expected,
                "Parsed value should match the expected enum value.");
        }

        [Test]
        public void TryParse_InvalidEnumStringWithCustomMapping_ReturnsTrueAndParsesCustomValue()
        {
            const string input = "CustomOption";
            const TestEnum expected = TestEnum.OptionB;
            TestTryParse(
                input,
                new Dictionary<string, TestEnum>() { { input, expected } },
                true,
                "The method should return true for a valid custom mapping.",
                expected,
                "Parsed value should match the expected custom enum value.");
        }

        [Test]
        public void TryParse_InvalidEnumStringWithoutCustomMapping_ReturnsFalseAndDefaultValue()
        {
            TestTryParse(
                "InvalidOption",
                new Dictionary<string, TestEnum>(),
                false,
                "The method should return false for an invalid string without a custom mapping.",
                default,
                "Parsed value should be set to the default value.");
        }

        [Test]
        public void TryParse_NullOrEmptyString_ReturnsTrueAndDefaultValue()
        {
            TestTryParse(
                null,
                new Dictionary<string, TestEnum>(),
                true,
                "The method should return true when the input string is null or empty.",
                default,
                "Parsed value should be set to the default value.");
        }

        [Test]
        public void TryParse_ListOfValidEnumStrings_ReturnsTrueAndCombinedValue()
        {
            // Arrange
            List<string> input = new() { "OptionA", "OptionB" };
            const TestEnum expected = TestEnum.OptionA | TestEnum.OptionB;
            var customMappings = new Dictionary<string, TestEnum>();

            // Act
            bool result = EnumUtility<TestEnum>.TryParse(input, customMappings, out TestEnum parsedValue);

            // Assert
            Assert.IsTrue(result, "The method should return true for a list of valid enum strings.");
            Assert.AreEqual(expected, parsedValue, "Parsed value should be the combination of the provided values.");
        }

        private static void TestTryParse(
            string input,
            IDictionary<string, TestEnum> customMappings,
            bool expectedResult, string unexpectedResultMessage,
            TestEnum expectedValue, string unexpectedValueMessage)
        {
            bool result = EnumUtility<TestEnum>.TryParse(input, customMappings, out TestEnum parsedValue);

            Assert.AreEqual(result, expectedResult, unexpectedResultMessage);
            Assert.AreEqual(parsedValue, expectedValue, unexpectedValueMessage);
        }

        [Test]
        public void GetEnumerator_ExtractsIndividualFlagsFromCombinedEnum()
        {
            // Arrange
            const TestEnum combinedValue = TestEnum.OptionB | TestEnum.OptionC;
            var expected = new List<TestEnum> { TestEnum.OptionB, TestEnum.OptionC };

            // Act
            var result = EnumUtility<TestEnum>.GetEnumerator(combinedValue).ToList();

            // Assert
            CollectionAssert.AreEquivalent(expected, result,
                "The enumerator should extract the individual flag values from the combined enum.");
        }
    }
}