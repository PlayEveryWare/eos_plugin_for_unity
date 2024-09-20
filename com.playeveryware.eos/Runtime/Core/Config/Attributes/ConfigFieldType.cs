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
        /// A plain double.
        /// </summary>
        Double,

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

}