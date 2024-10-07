/*
 * Copyright (c) 2021 PlayEveryWare
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
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
    using System.Text.RegularExpressions;
    using System;

    public class SandboxId : IEquatable<SandboxId>
    {
        private string _value;
        const string PreProductionEnvironmentRegex = @"^p\-[a-zA-Z\d]{30}$";

        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (!Guid.TryParse(value, out _))
                {
                    if (!Regex.IsMatch(value, PreProductionEnvironmentRegex))
                    {
                        throw new ArgumentException(
                            "Value for SandboxId must either be " +
                            "parseable to a Guid, or it must start with a " +
                            "lowercase 'p', followed by a dash and thirty " +
                            "letter characters.");
                    }
                }

                _value = value;
            }
        }

        public bool Equals(SandboxId other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((SandboxId)obj);
        }

        public override int GetHashCode()
        {
            return (_value?.GetHashCode()) ?? 0;
        }
    }
}