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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.IO.Compression;
using System;
using System.Linq;
using PlayEveryWare.EpicOnlineServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Playeveryware.Editor;
namespace Playeveryware.Editor
{
    [Serializable]
    public class SrcDestPair
    {
        // Allows for adding a comment
        [SerializeField]
        public string comment;

        [SerializeField]
        public bool recursive;
        [SerializeField]
        public string src;

        // If pattern is provided, src must be a directory,
        [SerializeField]
        public string pattern;

        [SerializeField]
        public string dest;

        // if this field is present, the build will error out if the SHA of the src file
        // doesn't match this. This allows as a reminder to update files
        [SerializeField]
        public string sha1;

        [SerializeField]
        public string sha1_mismatch_error;

        // Files matching this pattern will 
        [SerializeField]
        public string ignore_regex;

        // Do file copy even if dest file already exists and is identical
        [SerializeField]
        public bool copy_identical;

        // Normalizes file line endings to unix line endings.
        // One probably shouldn't do this for non-text files.
        [SerializeField]
        public bool dos2unix;

        public bool signWithDefaultCertificate;

        public bool IsCommentOnly()
        {
            return EmptyPredicates.IsEmptyOrNull(src) && EmptyPredicates.IsEmptyOrNull(dest) && EmptyPredicates.IsEmptyOrNull(ignore_regex);
        }
    }

    public class FileInfoMatchingResult
    {
        public SrcDestPair originalSrcDestPair;
        public FileInfo fileInfo;
        public string relativePath = null;
        public string GetDestination()
        {
            if (!string.IsNullOrWhiteSpace(relativePath) && (originalSrcDestPair.dest.EndsWith("/") || originalSrcDestPair.dest.Length == 0))
            {
                return Path.Combine(originalSrcDestPair.dest, relativePath);
            }
            return originalSrcDestPair.dest;
        }
    }

    [Serializable]
    public class PackageDescription
    {
        [SerializeField]
        public List<SrcDestPair> source_to_dest;

        [SerializeField]
        public List<string> blacklist;
    }
}
