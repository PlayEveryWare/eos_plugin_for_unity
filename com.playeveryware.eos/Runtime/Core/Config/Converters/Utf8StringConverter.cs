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

namespace PlayEveryWare.EpicOnlineServices.JsonConverters
{
    using Epic.OnlineServices;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    public class Utf8StringConverter : JsonConverter<Utf8String>
    {
        public override void WriteJson(JsonWriter writer, Utf8String value, JsonSerializer serializer)
        {
            if (value == null || value.Bytes == null)
            {
                writer.WriteNull();
                return;
            }

            // Start writing the array
            writer.WriteStartArray();

            foreach (byte b in value.Bytes)
            {
                writer.WriteValue(b);
            }

            writer.WriteEndArray();
        }

        public override Utf8String ReadJson(JsonReader reader, Type objectType, Utf8String existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonToken.StartArray)
            {
                throw new JsonSerializationException($"Unexpected token {reader.TokenType} when starting to deserialize Utf8String. Expected StartArray.");
            }

            var bytes = new List<byte>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndArray)
                {
                    break;
                }

                if (reader.TokenType != JsonToken.Integer)
                {
                    throw new JsonSerializationException($"Unexpected token {reader.TokenType} when reading bytes of Utf8String. Expected Integer.");
                }

                bytes.Add(Convert.ToByte(reader.Value));
            }

            if (bytes.Count == 0)
            {
                return null;
            }
            else
            {
                return new Utf8String(bytes.ToArray());
            }
        }
    }

}