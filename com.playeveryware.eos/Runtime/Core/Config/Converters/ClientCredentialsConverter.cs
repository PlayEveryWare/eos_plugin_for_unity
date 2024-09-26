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
    using Epic.OnlineServices.Platform;
    using Newtonsoft.Json;
    using System;

    public class ClientCredentialsConverter : JsonConverter<ClientCredentials>
    {
        public override ClientCredentials ReadJson(JsonReader reader, Type objectType, ClientCredentials existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            ClientCredentials result = new();

            if (reader.TokenType == JsonToken.Null)
            {
                return default; // Handle null JSON tokens
            }

            // Ensure we're at the start of an object
            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new JsonSerializationException($"Unexpected token {reader.TokenType} when starting to deserialize MyType.");
            }

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                {
                    return result;
                }

                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string propertyName = (string)reader.Value;

                    // Read the value token
                    if (!reader.Read())
                    {
                        throw new JsonSerializationException($"Unexpected end when reading {nameof(ClientCredentials)}.");
                    }

                    switch (propertyName)
                    {
                        case "ClientId":
                            result.ClientId = new Utf8String(reader.Value as byte[]);
                            break;
                        case "ClientSecret":
                            result.ClientSecret = new Utf8String(reader.Value as byte[]);
                            break;
                        default:
                            // Skip unknown properties
                            reader.Skip();
                            break;
                    }
                }
            }

            throw new JsonSerializationException("Unexpected end of JSON object.");
        }

        public override void WriteJson(JsonWriter writer, ClientCredentials value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("ClientId");
            writer.WriteValue(value.ClientId.Bytes);

            writer.WritePropertyName("ClientSecret");
            writer.WriteValue(value.ClientSecret.Bytes);

            writer.WriteEndObject();
        }
    }
}