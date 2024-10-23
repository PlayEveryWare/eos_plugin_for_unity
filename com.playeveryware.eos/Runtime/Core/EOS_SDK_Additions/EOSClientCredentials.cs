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
 *
 */

#if !EOS_DISABLE

namespace PlayEveryWare.EpicOnlineServices
{
    using Newtonsoft.Json;
    using System;
    using System.Security.Cryptography;

    public class EOSClientCredentials : IEquatable<EOSClientCredentials>
    {
        public string ClientId;
        public string ClientSecret;
        public readonly string EncryptionKey;

        public EOSClientCredentials()
        {
            // Randomly generate a 32 byte hex key
            byte[] randomBytes = new byte[32];
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            EncryptionKey = BitConverter.ToString(randomBytes).Replace(
                "-", string.Empty);
        }

        [JsonConstructor]
        public EOSClientCredentials(
            string clientId,
            string clientSecret,
            string encryptionKey)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            EncryptionKey = encryptionKey;
        }

        public bool Equals(EOSClientCredentials other)
        {
            if (other == null)
            {
                return false;
            }

            return ClientId == other.ClientId &&
                   ClientSecret == other.ClientSecret;
        }

        public override bool Equals(object other)
        {
            return other is EOSClientCredentials otherCreds && Equals(otherCreds);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ClientId, ClientSecret);
        }
    }
}

#endif