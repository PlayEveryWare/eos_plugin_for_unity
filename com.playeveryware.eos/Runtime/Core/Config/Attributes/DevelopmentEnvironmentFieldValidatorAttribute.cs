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

    public class DevelopmentEnvironmentFieldValidatorAttribute : FieldValidatorAttribute
    {
        const string PreProductionEnvironmentRegex = @"^p\-[a-zA-Z\d]{30}$";
        public const string FieldDidNotMatchMessage = "The field value is not a GUID, and did not match the regex used for Pre Production Environments: '" + PreProductionEnvironmentRegex + "'.";

        public override bool FieldValueIsValid(object toValidate, out string configurationProblemMessage)
        {
            if (!(toValidate is string singleStringValue))
            {
                configurationProblemMessage = $"The field value is not of type string.";
                return false;
            }

            if (string.IsNullOrEmpty(singleStringValue))
            {
                configurationProblemMessage = string.Empty;
                return true;
            }

            if (Regex.IsMatch(singleStringValue, PreProductionEnvironmentRegex))
            {
                configurationProblemMessage = string.Empty;
                return true;
            }

            if (Guid.TryParse(singleStringValue, out _))
            {
                configurationProblemMessage = string.Empty;
                return true;
            }

            configurationProblemMessage = FieldDidNotMatchMessage;
            return false;
        }

        bool isValidEntry(string toValidate)
        {
            if (string.IsNullOrEmpty(toValidate))
            {
                return true;
            }

            if (Regex.IsMatch(toValidate, PreProductionEnvironmentRegex))
            {
                return true;
            }

            if (Guid.TryParse(toValidate, out _))
            {
                return true;
            }

            return false;
        }
    }
}