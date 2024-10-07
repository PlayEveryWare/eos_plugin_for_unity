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
    using Common;
    using Epic.OnlineServices.Platform;
    using System;

    public class WrappedClientCredentials : WrappedStruct<ClientCredentials>, IEquatable<WrappedClientCredentials>
    {
        public WrappedClientCredentials(ClientCredentials credentials) : base(credentials) { }

        public override bool Equals(WrappedStruct<ClientCredentials> other)
        {
            if (other == null)
            {
                return false;
            }

            var temp = other.Unwrap();

            return temp.ClientId == _value.ClientId &&
                   temp.ClientSecret == _value.ClientSecret;
        }

        public bool Equals(WrappedClientCredentials other)
        {
            return Equals(other as WrappedStruct<ClientCredentials>);
        }
    }
}

#endif