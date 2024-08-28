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

namespace PlayEveryWare.EpicOnlineServices.Tests.Utility.Extensions
{
    using NUnit.Framework;
    using PlayEveryWare.EpicOnlineServices.Extensions;
    using System.Collections.Generic;
    using System.Linq;

    [TestFixture]
    public class ListExtensionsTests
    {
        /// <summary>
        /// Test to verify that the Shuffle method changes the order of elements in the list.
        /// </summary>
        [Test]
        public void Shuffle_ChangesOrderOfElements()
        {
            // Arrange
            List<int> originalList = Enumerable.Range(1, 100).ToList();
            List<int> shuffledList = new(originalList);

            // Act
            shuffledList.Shuffle();

            // Assert
            Assert.AreNotEqual(originalList, shuffledList,
                "The shuffled list should not be in the same order as the original list.");
            Assert.AreEqual(originalList.Count, shuffledList.Count,
                "The shuffled list should have the same number of elements as the original list.");
            Assert.IsTrue(originalList.All(shuffledList.Contains),
                "The shuffled list should contain all elements from the original list.");
        }

        /// <summary>
        /// Test to verify that the Shuffle method does not lose any elements.
        /// </summary>
        [Test]
        public void Shuffle_DoesNotLoseElements()
        {
            // Arrange
            List<int> originalList = new()
            {
                1,
                2,
                3,
                4,
                5
            };
            List<int> shuffledList = new(originalList);

            // Act
            shuffledList.Shuffle();

            // Assert
            CollectionAssert.AreEquivalent(originalList, shuffledList,
                "The shuffled list should contain the same elements as the original list.");
        }

        /// <summary>
        /// Test to verify that shuffling an empty list does not throw an exception.
        /// </summary>
        [Test]
        public void Shuffle_EmptyList_NoException()
        {
            // Arrange
            List<int> emptyList = new();

            // Act & Assert
            Assert.DoesNotThrow(() => emptyList.Shuffle(), "Shuffling an empty list should not throw an exception.");
        }

        /// <summary>
        /// Test to verify that shuffling a single-element list does not change its order.
        /// </summary>
        [Test]
        public void Shuffle_SingleElementList_NoChange()
        {
            // Arrange
            List<int> singleElementList = new() { 42 };

            // Act
            singleElementList.Shuffle();

            // Assert
            Assert.AreEqual(1, singleElementList.Count,
                "A single-element list should still have one element after shuffling.");
            Assert.AreEqual(42, singleElementList[0], "A single-element list should remain unchanged after shuffling.");
        }
    }
}
