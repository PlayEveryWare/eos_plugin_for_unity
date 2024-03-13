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

using UnityEngine;
using System;

namespace PlayEveryWare.EpicOnlineServices.Editor.Build
{
    /// <summary>
    /// Represents a task of copying source file(s) to certain destination.
    /// </summary>
    [Serializable]
    public class SrcDestPair
    {
        /// <summary>
        /// Represents a "comment" entry. Is used to visually add sections to the
        /// JSON file for easier human reading.
        /// </summary>
        [SerializeField] 
        public string comment;

        /// <summary>
        /// Source path that indicates a file or set of files to copy to a specific destination.
        /// Unless the SrcDestPair entry contains only a comment, this field is required.
        ///
        /// The src path may end with a double asterisk to indicate that the matching should be
        /// done recursively. This is a common pattern in a lot of tools, but is not the default
        /// behavior of most .NET implementations. For our purposes, it is a useful mechanism
        /// so we implement it's functionality.
        ///
        /// To explicitly exclude a file or set of files, preface the filepath provided with an
        /// exclamation mark. In the case where files are being excluded like this, the dest
        /// field should be left empty - as its value will be ignored.
        /// </summary>
        [SerializeField] 
        public string src;

        /// <summary>
        /// Indicates the destination that files matching the src pattern should be copied to
        /// when creating the package. If either the comment field is set, or if the src field's
        /// value is prefaced by an exclamation point, then the value of this field is ignored.
        /// </summary>
        [SerializeField] 
        public string dest;

        /// <summary>
        /// PLEASE SEE NOTE
        /// 
        /// An optional SHA1 hash to include for when the src field pattern only matches a single
        /// file. Optionally, this field can be set to the SHA1 hash of the file the last time it
        /// was copied.
        ///
        /// NOTE: Due to the changed nature of how packages are created, this particular field is
        ///       not widely utilized, and its use is not recommended, as it's preferable to copy
        ///       all relevant files overwriting the destination, to be sure that the latest files
        ///       are included in the package.
        /// </summary>
        [SerializeField] 
        public string sha1;

        /// <summary>
        /// In circumstances where the sha1 field is utilized, this field can optionally be
        /// included in-order to include a warning message unique to the file for the user
        /// who is creating the package.
        ///
        /// NOTE: Due to the changed nature of how packages are created, this particular field is
        ///       not widely utilized, and its use is not recommended, as it's preferable to copy
        ///       all relevant files overwriting the destination, to be sure that the latest files
        ///       are included in the package.
        /// </summary>
        [SerializeField] 
        public string sha1_mismatch_error;

        /// <summary>
        /// Indicates whether the particular SrcDestPair entry is only a comment. This is useful
        /// in-order to skip considering comment-only entries, while still allowing comment
        /// fields to be added to SrcDestPair entries where doing so is beneficial.
        /// </summary>
        /// <returns>True if the entry only contains a comment.</returns>
        public bool IsCommentOnly()
        {
            return (!string.IsNullOrEmpty(comment) &&
                    string.IsNullOrEmpty(src) &&
                    string.IsNullOrEmpty(dest));
        }
    }
}