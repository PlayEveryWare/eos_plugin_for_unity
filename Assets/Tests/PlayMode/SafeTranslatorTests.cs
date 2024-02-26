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
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using NUnit.Framework;

namespace PlayEveryWare.EpicOnlineServices.Editor.Utility
{
    public class SafeTranslatorTests
    {
        private delegate bool TryConvertDelegate<TInput, TOutput>(TInput input, out TOutput output);

        /// <summary>
        /// Tests a given conversionMethod using provided input and expected output.
        /// </summary>
        /// <typeparam name="TInput">The input type.</typeparam>
        /// <typeparam name="TOutput">The output type (opposite sign of the input type).</typeparam>
        /// <param name="inputValue">The input value.</param>
        /// <param name="expectedResult">Whether or not the conversion is successful.</param>
        /// <param name="expectedOutput">The expected value of output after the conversion is attempted.</param>
        /// <param name="conversionMethod">The method of conversion to use.</param>
        private static void TestTryConvert<TInput, TOutput>(
            TInput inputValue, 
            bool expectedResult, 
            TOutput expectedOutput, 
            TryConvertDelegate<TInput, TOutput> conversionMethod)
        {
            bool converted = conversionMethod(inputValue, out TOutput output);

            Assert.AreEqual(expectedResult, converted, $"Expected conversion result to be {expectedResult}.");
            Assert.AreEqual(expectedOutput, output, $"Expected output to be {expectedOutput}.");
        }

        /// <summary>
        /// Tests that when you try to convert a negative int to a uint, the conversion
        /// fails, and the output value is set to a straightforward cast.
        /// </summary>
        [Test]
        public static void TryConvert_IntToUint_NegativeInt_ReturnsFalse()
        {
            const int negativeValue = -1;
            const uint uncheckedCast = unchecked((uint)negativeValue);

            TestTryConvert(negativeValue, false, uncheckedCast, SafeTranslator.TryConvert);
        }

        /// <summary>
        /// Tests that an int can be converted safely to a uint when the int value
        /// is set to int.MaxValue.
        /// </summary>
        [Test]
        public static void TryConvert_IntToUint_PositiveInt_ReturnsTrue()
        {
            const int positiveValue = int.MaxValue;
            const uint uncheckedCast = unchecked((uint)positiveValue);

            TestTryConvert(positiveValue, true, uncheckedCast, SafeTranslator.TryConvert);
        }

        /// <summary>
        /// Test that trying to convert a uint with a value that is higher than int.MaxValue
        /// results in the TryConvert function returning false, and the output value being
        /// set to what a straightforward.
        /// </summary>
        [Test]
        public static void TryConvert_UintToInt_OverflowInt_ReturnsFalse()
        {
            const uint overflowValue = uint.MaxValue;
            const int uncheckedCast = unchecked((int)overflowValue);

            TestTryConvert(overflowValue, false, uncheckedCast, SafeTranslator.TryConvert);
        }

        /// <summary>
        /// Test that trying to convert a uint to int within range works properly (returns
        /// true, and has output set to unchecked cast.
        /// </summary>
        [Test]
        public static void TryConvert_UintToInt_PositiveInt_ReturnsTrue()
        {
            const uint positiveValue = 55;
            const int uncheckedCast = unchecked((int)positiveValue);

            TestTryConvert(positiveValue, true, uncheckedCast, SafeTranslator.TryConvert);
        }

        [Test]
        public static void TryConvert_LongToUlong_NegativeLong_ReturnsFalse()
        {
            const long negativeValue = -1;
            const ulong uncheckedCast = unchecked((ulong)negativeValue);

            TestTryConvert(negativeValue, false, uncheckedCast, SafeTranslator.TryConvert);
        }

        [Test]
        public static void TryConvert_LongToULong_PositiveLong_ReturnsTrue()
        {
            const long positiveValue = 55;
            const ulong uncheckedCast = unchecked((ulong)positiveValue);

            TestTryConvert(positiveValue, true, uncheckedCast, SafeTranslator.TryConvert);
        }

        [Test]
        public static void TryConvert_ULongToLong_OverflowLong_ReturnsFalse()
        {
            const ulong overflowValue = ulong.MaxValue;
            const long uncheckedCast = unchecked((long)overflowValue);

            TestTryConvert(overflowValue, false, uncheckedCast, SafeTranslator.TryConvert);
        }

        [Test]
        public static void TryConvert_ULongToLong_PositiveLong_ReturnsTrue()
        {
            const long positiveValue = 55;
            const ulong uncheckedCast = unchecked((ulong)positiveValue);

            TestTryConvert(positiveValue, true, uncheckedCast, SafeTranslator.TryConvert);
        }
    }
}
