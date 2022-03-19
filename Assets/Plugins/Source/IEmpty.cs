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

using System.Collections.Generic;

namespace PlayEveryWare.EpicOnlineServices
{
    public interface IEmpty
    {
        bool IsEmpty();
    }

    public static class EmptyPredicates
    {
        public static bool IsEmptyOrNull(string s)
        {
            return s == null || s.Length == 0;
        }

        public static bool IsEmptyOrNull(bool? b)
        {
            return b == null;
        }

        public static bool IsEmptyOrNull<T>(List<T> list)
        {
            return list == null || list.Count == 0;
        }

        public static bool IsEmptyOrNull(IEmpty o)
        {
            return o == null || o.IsEmpty();
        }

        public static bool IsEmptyOrNullOrContainsOnlyEmpty(List<string> list)
        {
            return list == null || list.Count == 0 || list.TrueForAll(IsEmptyOrNull);
        }

        public static bool IsEmptyOrContainsOnlyEmpty(List<string> list)
        {
            return list.Count == 0 || list.TrueForAll(IsEmptyOrNull);
        }

        public static T NewIfNull<T>(T value) where T : new()
        {
            if (value == null)
            {
                return new T();
            }
            return value;
        }

        public static string NewIfNull(string value)
        {
            return value == null ? "" : value;
        }
    }
}
