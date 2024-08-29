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

namespace PlayEveryWare.EpicOnlineServices.Editor.Build
{
    using UnityEditor.Build.Reporting;

    /// <summary>
    /// Interface for platform specific builders.
    /// </summary>
    public interface IPlatformSpecificBuilder
    {
        /// <summary>
        /// Called by the BuildRunner before the build happens on the platform.
        /// </summary>
        /// <param name="report">The BuildReport for the prebuild task.</param>
        public void PreBuild(BuildReport report);

        /// <summary>
        /// Called by the BuildRunner after the build happens on the platform.
        /// </summary>
        /// <param name="report">The BuildReport for the postbuild task.</param>
        public void PostBuild(BuildReport report);

        public void BuildNativeCode();
    }
}