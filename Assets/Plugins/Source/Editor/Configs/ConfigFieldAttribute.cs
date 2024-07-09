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
namespace PlayEveryWare.EpicOnlineServices.Editor
{
    using System;

    public enum ConfigFieldType
    {
        Text,
        FilePath,
        DirectoryPath,
        Flag,
        Uint,
        Ulong
    }

    public class FilePathField : ConfigFieldAttribute
    {
        public string Extension { get; }

        public FilePathField(string label, string extension, int group = -1) : this(label, extension, null, group) { }

        public FilePathField(
            string label, 
            string extension, 
            string tooltip, 
            int group = -1) : base(label, ConfigFieldType.FilePath, tooltip, group)
        {
            Extension = extension;
        }
    }

    public class DirectoryPathField : ConfigFieldAttribute
    {
        public DirectoryPathField(string label, int group = -1) : base(label, ConfigFieldType.DirectoryPath, group) { }

        public DirectoryPathField(string label, string tooltip, int group = -1) : base(label, ConfigFieldType.DirectoryPath, tooltip, group) { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigGroupAttribute : Attribute
    {
        public string Label { get; }

        public ConfigGroupAttribute(string label)
        {
            Label = label;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ConfigFieldAttribute : Attribute
    {   
        public string Label { get; }

        public string ToolTip { get; }

        public int Group { get; }

        public ConfigFieldType FieldType { get; }

        public ConfigFieldAttribute(string label, ConfigFieldType type) : this(label, type, null, -1)
        {

        }

        public ConfigFieldAttribute(string label, ConfigFieldType type, int group = -1) : this(label, type, null, group)
        {
        }

        public ConfigFieldAttribute(string label, ConfigFieldType type, string tooltip, int group = -1)
        {
            Label = label;
            ToolTip = tooltip;
            Group = group;
            FieldType = type;
        }
    }
}