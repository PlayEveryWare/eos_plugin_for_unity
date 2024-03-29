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
namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using System;
    using UnityEngine.UI;

    public static class DropdownExtensions
    {
        public static bool TryGetSelectedEnum<T>(this Dropdown dropdown, out T selected) where T : struct, Enum
        {
            var selectedString = dropdown.options[dropdown.value].text;

            UnityEngine.Debug.Log($"Selected string is: \"{selectedString}\".");
            if (Enum.TryParse(selectedString, out T value))
            {
                selected = value;
                return true;
            }

            selected = default;
            return false;
        }

        public static T GetSelectedEnum<T>(this Dropdown dropdown) where T : struct, Enum
        {
            if (TryGetSelectedEnum(dropdown, out T typeSelected))
            {
                return typeSelected;
            }
            return default;
        }
    }
}