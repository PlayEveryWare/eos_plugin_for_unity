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

    [AttributeUsage(AttributeTargets.Field)]
    public class ConfigFieldAttribute : Attribute
    {   
        /// <summary>
        /// The label for the config field.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// The tooltip to display when the user hovers a mouse over the label.
        /// </summary>
        public string ToolTip { get; }

        /// <summary>
        /// The group that that config field belongs to (helps to cluster config
        /// fields into meaningful groups).
        /// </summary>
        public int Group { get; }

        /// <summary>
        /// The type of the field - used to inform how to render input controls
        /// and validation.
        /// </summary>
        public ConfigFieldType FieldType { get; }

        public ConfigFieldAttribute(
            string label, 
            ConfigFieldType type, 
            string tooltip = null, 
            int group = -1)
        {
            Label = label;
            ToolTip = tooltip;
            Group = group;
            FieldType = type;
        }
    }
}