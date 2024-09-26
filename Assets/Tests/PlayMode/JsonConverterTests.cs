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

namespace PlayEveryWare.EpicOnlineServices.Tests
{
    using Epic.OnlineServices;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using PlayEveryWare.EpicOnlineServices.JsonConverters;
    using System;
    using System.Text;
    using UnityEngine.UI;

    [TestFixture]
    public class Utf8StringConverterTests
    {
        private JsonSerializerSettings _settings;

        [SetUp]
        public void Setup()
        {
            _settings = new JsonSerializerSettings();
            _settings.Converters.Add(new Utf8StringConverter());
        }

        [Test]
        public void Serialize_Utf8String_With_Bytes_Writes_Byte_Array()
        {
            // Arrange
            var utf8String = new Utf8String(new byte[] { 72, 101, 108, 108, 111, 0 });

            // Act
            var json = JsonConvert.SerializeObject(utf8String, _settings);

            // Assert
            Assert.AreEqual("[72,101,108,108,111,0]", json);
        }

        [Test]
        public void Serialize_Utf8String_With_String_Deserializes_To_Same_String()
        {
            string normalString = "blah blah blah";
            
            Utf8String test = new Utf8String(normalString);

            var json = JsonConvert.SerializeObject(test, _settings);

            Utf8String deserializedTest = JsonConvert.DeserializeObject<Utf8String>(json, _settings);

            // TODO: I would argue that this is a bug in Epic's code (because this test fails)
            Assert.AreEqual(normalString, deserializedTest.ToString());
        }

        [Test]
        public void Serialize_Null_Utf8String_Writes_Null()
        {
            // Arrange
            Utf8String utf8String = null;

            // Act
            var json = JsonConvert.SerializeObject(utf8String, _settings);

            // Assert
            Assert.AreEqual("null", json);
        }

        [Test]
        public void Serialize_Utf8String_With_Null_Bytes_Writes_Null()
        {
            string nullString = null;

            // Arrange
            var utf8String = new Utf8String(nullString);

            // Act
            var json = JsonConvert.SerializeObject(utf8String, _settings);

            // Assert
            Assert.AreEqual("null", json);
        }

        [Test]
        public void Deserialize_Valid_Json_Array_To_Utf8String()
        {
            // Arrange
            var json = "[72,101,108,108,111,0]";

            // Act
            var utf8String = JsonConvert.DeserializeObject<Utf8String>(json, _settings);

            // Assert
            Assert.IsNotNull(utf8String);
            CollectionAssert.AreEqual(new byte[] { 72, 101, 108, 108, 111, 0 }, utf8String.Bytes);
        }

        [Test]
        public void Deserialize_Null_Json_To_Null_Utf8String()
        {
            // Arrange
            var json = "null";

            // Act
            var utf8String = JsonConvert.DeserializeObject<Utf8String>(json, _settings);

            // Assert
            Assert.IsNull(utf8String);
        }

        [Test]
        public void Deserialize_Invalid_Token_Throws_Exception()
        {
            // Arrange
            var json = "\"invalid\"";

            // Act & Assert
            var ex = Assert.Throws<JsonSerializationException>(() =>
                JsonConvert.DeserializeObject<Utf8String>(json, _settings));

            StringAssert.Contains("Unexpected token", ex.Message);
        }

        [Test]
        public void Deserialize_Array_With_NonInteger_Element_Throws_Exception()
        {
            // Arrange
            var json = "[72,101,\"invalid\",108,111]";

            // Act & Assert
            var ex = Assert.Throws<JsonSerializationException>(() =>
                JsonConvert.DeserializeObject<Utf8String>(json, _settings));

            StringAssert.Contains("Unexpected token", ex.Message);
        }

        [Test]
        public void Deserialize_Empty_Array_Returns_NullUtf8String()
        {
            // Arrange
            var json = "[]";

            // Act
            var utf8String = JsonConvert.DeserializeObject<Utf8String>(json, _settings);

            // Assert
            Assert.IsNull(utf8String);
        }

        [Test]
        public void Serialize_Utf8String_With_Empty_Bytes_Writes_Empty_Array()
        {
            // Arrange
            var utf8String = new Utf8String(new byte[1] { 0 });

            // Act
            var json = JsonConvert.SerializeObject(utf8String, _settings);

            // Assert
            Assert.AreEqual("[0]", json);
        }

        [Test]
        public void Deserialize_NonArray_StartToken_Throws_Exception()
        {
            // Arrange
            var json = "{ \"Bytes\": [72,101,108,108,111] }";

            // Act & Assert
            var ex = Assert.Throws<JsonSerializationException>(() =>
                JsonConvert.DeserializeObject<Utf8String>(json, _settings));

            StringAssert.Contains("Unexpected token", ex.Message);
        }

        [Test]
        public void Deserialize_Partial_Array_Throws_Exception()
        {
            // Arrange
            var json = "[72,101,108,108";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                JsonConvert.DeserializeObject<Utf8String>(json, _settings));

            StringAssert.Contains("not null terminated", ex.Message);
        }
    }


}