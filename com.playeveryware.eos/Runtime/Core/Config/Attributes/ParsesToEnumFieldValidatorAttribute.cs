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
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    [AttributeUsage(AttributeTargets.Field)]

    public abstract class ParsesToEnumFieldValidatorAttribute : FieldValidatorAttribute
    {
        public sealed override bool FieldValueIsValid(object toValidate, out string configurationProblemMessage)
        {
            List<string> flags = new List<string>();

            if ((toValidate is string singleStringValue))
            {
                flags.Add(singleStringValue);
            }
            else if ((toValidate is List<string> listValues))
            {
                flags = new List<string>(listValues);
            }
            else
            {
                configurationProblemMessage = $"Value is neither a string or a List<string>.";
                return false;
            }

            if (!CanParse(flags))
            {
                configurationProblemMessage = $"Failed to parse all provided values into destination enum flags.";
                return false;
            }

            configurationProblemMessage = string.Empty;
            return true;
        }

        protected abstract bool CanParse(List<string> flags);
    }
}