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

namespace PlayEveryWare.EpicOnlineServices
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.ComponentModel;

    public class StringToTypeConverter<T> : JsonConverter
    {
        private readonly Type _targetType;
        private readonly Type _underlyingType;

        public StringToTypeConverter()
        {
            _targetType = typeof(T);
            _underlyingType = Nullable.GetUnderlyingType(_targetType) ?? _targetType;
        }

        public override bool CanConvert(Type objectType)
        {
            return _targetType.IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);

            // Handle null tokens for nullable types
            if (token.Type == JTokenType.Null && Nullable.GetUnderlyingType(objectType) != null)
            {
                return null;
            }

            try
            {
                // Convert the token to the target type
                return ConvertToken(token, _underlyingType);
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException($"Error converting token '{token}' to type '{_targetType}'.", ex);
            }
        }

        private static object ConvertToken(JToken token, Type targetType)
        {
            string stringValue = token.Value<string>();
            if (string.IsNullOrEmpty(stringValue))
            {
                stringValue = default(T).ToString();
            }

            switch (token.Type)
            {
                case JTokenType.String:
                    if (targetType.IsEnum)
                    {
                        return Enum.Parse(targetType, stringValue);
                    }

                    if (targetType == typeof(Guid))
                    {
                        return Guid.Parse(stringValue);
                    }
                    else
                    {
                        // Use TypeConverter for other types
                        var converter = TypeDescriptor.GetConverter(targetType);
                        return converter.ConvertFromInvariantString(stringValue);
                    }
                case JTokenType.Integer:
                case JTokenType.Float:
                    
                    // Directly convert numeric types
                    return token.ToObject(targetType);
                case JTokenType.Guid when targetType == typeof(Guid):
                    return token.ToObject<Guid>();
                case JTokenType.None:
                case JTokenType.Object:
                case JTokenType.Array:
                case JTokenType.Constructor:
                case JTokenType.Property:
                case JTokenType.Comment:
                case JTokenType.Boolean:
                case JTokenType.Null:
                case JTokenType.Undefined:
                case JTokenType.Date:
                case JTokenType.Raw:
                case JTokenType.Bytes:
                case JTokenType.Uri:
                case JTokenType.TimeSpan:
                default:
                    throw new JsonSerializationException($"Unexpected token type '{token.Type}' when parsing to type '{targetType}'.");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Serialize the value using the default serializer
            serializer.Serialize(writer, value);
        }
    }
}