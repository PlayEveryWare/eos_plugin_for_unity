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

namespace PlayEveryWare.EpicOnlineServices.Tests.Config
{
    using NUnit.Framework;
    using Newtonsoft.Json;
    using System;

    public static partial class StringToTypeConverterTests
    {
        private enum TestEnum
        {
            None,
            First,
            Second
        }

        private class TestClass
        {
            [JsonConverter(typeof(StringToTypeConverter<ulong>))]
            public ulong ULongValue { get; set; }

            [JsonConverter(typeof(StringToTypeConverter<ulong?>))]
            public ulong? NullableULongValue { get; set; }

            [JsonConverter(typeof(StringToTypeConverter<Guid>))]
            public Guid GuidValue { get; set; }

            [JsonConverter(typeof(StringToTypeConverter<Guid?>))]
            public Guid? NullableGuidValue { get; set; }

            [JsonConverter(typeof(StringToTypeConverter<TestEnum>))]
            public TestEnum EnumValue { get; set; }
        }

        [TestFixture]
        internal partial class StringToTypeConverterUnitTests
        {
            [Test]
            public void Deserialize_ULong_FromString()
            {
                const string json = "{ 'ULongValue': '1234567890' }";
                var obj = JsonConvert.DeserializeObject<TestClass>(json);
                Assert.AreEqual(1234567890UL, obj.ULongValue);
            }

            [Test]
            public void Deserialize_NullableULong_FromString()
            {
                const string json = "{ 'NullableULongValue': '9876543210' }";
                var obj = JsonConvert.DeserializeObject<TestClass>(json);
                Assert.AreEqual(9876543210UL, obj.NullableULongValue);
            }

            [Test]
            public void Deserialize_NullableULong_FromNull()
            {
                const string json = "{ 'NullableULongValue': null }";
                var obj = JsonConvert.DeserializeObject<TestClass>(json);
                Assert.IsNull(obj.NullableULongValue);
            }

            [Test]
            public void Deserialize_Guid_FromString()
            {
                const string guidString = "550e8400-e29b-41d4-a716-446655440000";
                string json = $"{{ 'GuidValue': '{guidString}' }}";
                var obj = JsonConvert.DeserializeObject<TestClass>(json);
                Assert.AreEqual(Guid.Parse(guidString), obj.GuidValue);
            }

            [Test]
            public void Deserialize_NullableGuid_FromString()
            {
                const string guidString = "d2719a31-2b76-4b47-bb39-d5ac8b8a5c1f";
                string json = $"{{ 'NullableGuidValue': '{guidString}' }}";
                var obj = JsonConvert.DeserializeObject<TestClass>(json);
                Assert.AreEqual(Guid.Parse(guidString), obj.NullableGuidValue);
            }

            [Test]
            public void Deserialize_NullableGuid_FromNull()
            {
                const string json = "{ 'NullableGuidValue': null }";
                var obj = JsonConvert.DeserializeObject<TestClass>(json);
                Assert.IsNull(obj.NullableGuidValue);
            }

            [Test]
            public void Deserialize_Enum_FromString()
            {
                const string json = "{ 'EnumValue': 'Second' }";
                var obj = JsonConvert.DeserializeObject<TestClass>(json);
                Assert.AreEqual(TestEnum.Second, obj.EnumValue);
            }

            [Test]
            public void Deserialize_InvalidULong_ThrowsException()
            {
                const string json = "{ 'ULongValue': 'invalid_number' }";
                Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<TestClass>(json));
            }

            [Test]
            public void Deserialize_InvalidGuid_ThrowsException()
            {
                const string json = "{ 'GuidValue': 'not_a_guid' }";
                Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<TestClass>(json));
            }

            [Test]
            public void Serialize_ULongValue()
            {
                var obj = new TestClass { ULongValue = 1234567890UL };
                string json = JsonConvert.SerializeObject(obj);
                StringAssert.Contains(@"""ULongValue"":1234567890", json);
            }

            [Test]
            public void Serialize_NullableULongValue_Null()
            {
                var obj = new TestClass { NullableULongValue = null };
                string json = JsonConvert.SerializeObject(obj);
                StringAssert.Contains(@"""NullableULongValue"":null", json);
            }

            [Test]
            public void Serialize_GuidValue()
            {
                var guid = Guid.NewGuid();
                var obj = new TestClass { GuidValue = guid };
                string json = JsonConvert.SerializeObject(obj);
                StringAssert.Contains($@"""GuidValue"":""{guid}""", json);
            }

            [Test]
            public void Serialize_EnumValue()
            {
                var obj = new TestClass { EnumValue = TestEnum.First };
                string json = JsonConvert.SerializeObject(obj);
                StringAssert.Contains(@"""EnumValue"":1", json);
            }

            [Test]
            public void Deserialize_ULong_FromNumber()
            {
                const string json = "{ 'ULongValue': 1234567890 }";
                var obj = JsonConvert.DeserializeObject<TestClass>(json);
                Assert.AreEqual(1234567890UL, obj.ULongValue);
            }

            [Test]
            public void Deserialize_Enum_FromNumber()
            {
                const string json = "{ 'EnumValue': 2 }"; // Second = 2
                var obj = JsonConvert.DeserializeObject<TestClass>(json);
                Assert.AreEqual(TestEnum.Second, obj.EnumValue);
            }
        }
    }
}