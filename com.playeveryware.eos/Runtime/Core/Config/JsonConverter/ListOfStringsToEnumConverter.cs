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
    using Epic.OnlineServices.Auth;
    using Epic.OnlineServices.Platform;
    using Epic.OnlineServices.UI;
    using Extensions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using Utility;

    internal class ListOfStringsToInputStateButtonFlags : ListOfStringsToEnumConverter<InputStateButtonFlags>
    {
        protected override InputStateButtonFlags FromStringArray(JArray array)
        {
            return FromStringArrayWithCustomMapping(array, null);
        }

        protected override InputStateButtonFlags FromNumberValue(JToken token)
        {
            return (InputStateButtonFlags)token.Value<int>();
        }
    }

    internal class ListOfStringsToAuthScopeFlags : ListOfStringsToEnumConverter<AuthScopeFlags>
    {
        protected override AuthScopeFlags FromStringArray(JArray array)
        {
            return FromStringArrayWithCustomMapping(array, AuthScopeFlagsExtensions.CustomMappings);
        }

        protected override AuthScopeFlags FromNumberValue(JToken token)
        {
            return (AuthScopeFlags)token.Value<int>();
        }
    }

    internal class ListOfStringsToPlatformFlags : ListOfStringsToEnumConverter<WrappedPlatformFlags>
    {
        protected override WrappedPlatformFlags FromStringArray(JArray array)
        {
            Dictionary<string, WrappedPlatformFlags> wrappedCustomMapping = new();
            foreach ((string key, PlatformFlags value) in PlatformFlagsExtensions.CustomMappings)
            {
                wrappedCustomMapping[key] = value.Wrap();
            }

            return FromStringArrayWithCustomMapping(array, wrappedCustomMapping);
        }

        protected override WrappedPlatformFlags FromNumberValue(JToken token)
        {
            return (WrappedPlatformFlags)token.Value<int>();
        }
    }

    internal abstract class ListOfStringsToEnumConverter<T> : JsonConverter where T : struct, Enum
    {
        private readonly Type _targetType = typeof(T);

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsEnum;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);

            return token.Type switch
            {
                JTokenType.Array => FromStringArray(token as JArray),

                JTokenType.String => Enum.Parse(
                    _targetType,
                    token.Value<string>()),

                JTokenType.Integer => FromNumberValue(token),

                JTokenType.Null => default(T),

                _ => throw new JsonSerializationException(
                    $"Unexpected token type '{token.Type}' when " +
                    $"parsing to type '{_targetType}'."),
            };
        }

        protected abstract T FromStringArray(JArray array);

        protected abstract T FromNumberValue(JToken token);

        protected T FromStringArrayWithCustomMapping(JArray array, IDictionary<string, T> customMappings)
        {
            List<string> elementsAsStrings = new();
            foreach (JToken element in array)
            {
                elementsAsStrings.Add(element.ToString());
            }

            _ = EnumUtility<T>.TryParse(elementsAsStrings, customMappings, out T result);

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Serialize the value using the default serializer
            serializer.Serialize(writer, value);
        }
    }
}