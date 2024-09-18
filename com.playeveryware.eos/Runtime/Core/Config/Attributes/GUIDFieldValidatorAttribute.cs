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
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Field)]

    public class GUIDFieldValidatorAttribute : FieldValidatorAttribute
    {
        public const string NotAGuidMessage = "The field value could not be parsed into a Guid.";

        public override bool FieldValueIsValid(object toValidate, out string configurationProblemMessage)
        {
            if (!(toValidate is string stringValue))
            {
                configurationProblemMessage = "The field value is not of type string.";
                return false;
            }

            if (string.IsNullOrEmpty(stringValue))
            {
                configurationProblemMessage = "The field value is an empty string.";
                return false;
            }

            if (!Guid.TryParse(stringValue, out Guid result))
            {
                configurationProblemMessage = NotAGuidMessage;
                return false;
            }

            configurationProblemMessage = string.Empty;
            return true;
        }
    }
}