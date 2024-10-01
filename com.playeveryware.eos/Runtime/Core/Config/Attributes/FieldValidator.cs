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

    public struct FieldValidatorFailure
    {
        public FieldInfo FieldInfo;
        public FieldValidatorAttribute FailingAttribute;
        public string FailingMessage;

        public FieldValidatorFailure(FieldInfo failingField,
            FieldValidatorAttribute failingAttribute,
            string failingMessage)
        {
            FieldInfo = failingField;
            FailingAttribute = failingAttribute;
            FailingMessage = failingMessage;
        }
    }

    public class FieldValidator
    {
        /// <summary>
        /// Call Field Validator on a single object value.
        /// </summary>
        /// <param name="fieldInfo">The FieldInfo that contains the attributes.</param>
        /// <param name="singularValue">Value of a smallest unit of a class, such as one element of a list.</param>
        /// <returns></returns>
        public static List<FieldValidatorFailure> GetFailingValidatorAttributeOnObject(FieldInfo fieldInfo, object singularValue)
        {
            List<FieldValidatorFailure> failingValidatorAttributes = new();

            if (fieldInfo.GetCustomAttribute(typeof(ExpandFieldAttribute)) != null)
            {
                failingValidatorAttributes.AddRange(GetFailingValidatorAttributeOnClass(singularValue));
            }
            else
            {
                foreach (FieldValidatorAttribute validatorAttribute in fieldInfo.GetCustomAttributes(typeof(FieldValidatorAttribute)))
                {
                    if (!validatorAttribute.FieldValueIsValid(singularValue, out string message))
                    {
                        failingValidatorAttributes.Add(new FieldValidatorFailure(fieldInfo, validatorAttribute, message));
                    }
                }
            }

            return failingValidatorAttributes;
        }

        /// <summary>
        /// Call Field Validator on a field within a class.
        /// If the field is a single variable, attempt validation.
        /// If the field is a list, it will treat each element individually.
        /// </summary>
        /// <param name="fieldInfo">
        /// The FieldInfo that contains the attributes. 
        /// Field that is a class or a list of classes should be marked with attribute "Expand".
        /// </param>
        /// <param name="fieldValue">Value of a field within a class.</param>
        /// <returns></returns>
        public static List<FieldValidatorFailure> GetFailingValidatorAttributeOnField(FieldInfo fieldInfo, object fieldValue)
        {
            List<FieldValidatorFailure> failingValidatorAttributes = new();

            if (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                List<object> collection = new List<object>((IEnumerable<object>)fieldValue);

                foreach (var element in collection)
                {
                    failingValidatorAttributes.AddRange(GetFailingValidatorAttributeOnObject(fieldInfo, element));
                }
            }
            else
            {
                failingValidatorAttributes.AddRange(GetFailingValidatorAttributeOnObject(fieldInfo, fieldValue));
            }

            return failingValidatorAttributes;
        }

        /// <summary>
        /// Call Field Validator on a class.
        /// Finds all fields within a class, validates each field.
        /// </summary>
        /// <param name="target">Target class to validate.</param>
        /// <returns></returns>
        public static List<FieldValidatorFailure> GetFailingValidatorAttributeOnClass(object target)
        {
            List<FieldValidatorFailure> failingValidatorAttributes = new();

            foreach (FieldInfo curField in target.GetType().GetFields())
            {
                failingValidatorAttributes.AddRange(GetFailingValidatorAttributeOnField(curField, curField.GetValue(target)));
            }

            return failingValidatorAttributes;
        }

        public static bool TryGetFailingValidatorAttributes(object target, out List<FieldValidatorFailure> failingValidatorAttributes)
        {
            failingValidatorAttributes = new List<FieldValidatorFailure>();

            failingValidatorAttributes.AddRange(GetFailingValidatorAttributeOnClass(target));

            return failingValidatorAttributes.Count > 0;
        }
    }
}