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

    public class InitializeThreadAffinityConverter : JsonConverter<InitializeThreadAffinity>
    {
        public override InitializeThreadAffinity ReadJson(JsonReader reader, Type objectType, InitializeThreadAffinity existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, InitializeThreadAffinity value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("StorageIo");
            writer.WriteValue(value.StorageIo);

            writer.WritePropertyName("RTCIo");
            writer.WriteValue(value.RTCIo);

            writer.WritePropertyName("P2PIo");
            writer.WriteValue(value.P2PIo);

            writer.WritePropertyName("HttpRequestIo");
            writer.WriteValue(value.HttpRequestIo);

            writer.WritePropertyName("NetworkWork");
            writer.WriteValue(value.NetworkWork);

            writer.WritePropertyName("WebSocketIo");
            writer.WriteValue(value.WebSocketIo);

            writer.WriteEndObject();
        }
    }
}