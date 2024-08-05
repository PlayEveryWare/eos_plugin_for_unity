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

    /// <summary>
    /// Indicates the type of the config field. This differs from the data type
    /// because a string value may represent a string, or it might more
    /// specifically represent a filepath. Whether to render the input as one
    /// or the other depends on the value here.
    /// </summary>
    public enum ConfigFieldType
    {
        /// <summary>
        /// A plain string value.
        /// </summary>
        Text,

        /// <summary>
        /// A string value that is to represent a path to a file.
        /// </summary>
        FilePath,

        /// <summary>
        /// A string value that is to represent a path to a directory.
        /// </summary>
        DirectoryPath,

        /// <summary>
        /// A plain boolean value.
        /// </summary>
        Flag,

        /// <summary>
        /// A plain unsigned integer.
        /// </summary>
        Uint,

        /// <summary>
        /// A plain unsigned long.
        /// </summary>
        Ulong,

        /// <summary>
        /// A list of strings.
        /// </summary>
        TextList,

        /// <summary>
        /// Indicates that the config has a button that needs rendering. This is
        /// a ConfigFieldType because a Config can have a field member that is
        /// an action (assigned functionality by the Config constructor) that is
        /// to take place when the button is pressed, which allows injection of
        /// arbitrary functionality within a ConfigEditor.
        /// </summary>
        Button,
    }

    #region Custom attribute classes that derive from ConfigFieldAttribute

    /// <summary>
    /// This attribute is used to decorate a field member within a config class
    /// that represents a path to a file.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class FilePathField : ConfigFieldAttribute
    {
        /// <summary>
        /// When launching the file selector in the system, use this to
        /// filter which files can be selected.
        /// </summary>
        public string Extension { get; }

        public FilePathField(string label, string extension, string tooltip = null, int group = -1) : base(label,
            ConfigFieldType.FilePath, tooltip, group)
        {
            Extension = extension;
        }
    }

    /// <summary>
    /// This attribute is used to decorate a field member within a config class
    /// that represents a path to a directory.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class DirectoryPathField : ConfigFieldAttribute
    {
        public DirectoryPathField(string label, string tooltip = null, int group = -1) : base(label, ConfigFieldType.DirectoryPath, tooltip, group) { }
    }

    /// <summary>
    /// This attribute is used to decorate a field member within a config class
    /// that can execute a callback on the config class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ButtonField : ConfigFieldAttribute
    {
        public ButtonField(string label, string tooltip = null, int group = -1) : base(label,
            ConfigFieldType.Button, tooltip, group)
        {
        }
    }

    #endregion

    /// <summary>
    /// This attribute is used to decorate a config class that represents a
    /// collection of configuration fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigGroupAttribute : Attribute
    {
        /// <summary>
        /// The label for the collection of config fields that the config class
        /// represents.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Indicates whether the group of configuration values can be
        /// collapsed.
        /// </summary>
        public bool Collapsible { get; }

        public ConfigGroupAttribute(string label, bool collapsible = false)
        {
            Label = label;
            Collapsible = collapsible;
        }
    }

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

        public ConfigFieldType FieldType { get; }
        
        public ConfigFieldAttribute(string label, ConfigFieldType type, string tooltip = null, int group = -1)
        {
            Label = label;
            ToolTip = tooltip;
            Group = group;
            FieldType = type;
        }
    }
}