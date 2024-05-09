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

using NUnit.Framework;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayEveryWare.EpicOnlineServices.Tests.Common
{
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Utility to perform common reflection tasks used across some of the unit tests.
    /// </summary>
    public class ReflectionUtility
    {
        /// <summary>
        /// Finds all non-abstract derived classes of a specified type within the same assembly.
        /// </summary>
        /// <typeparam name="TBase">The type to find derived classes for.</typeparam>
        /// <returns>A list of types that are non-abstract and derived from the specified base type.</returns>
        public static List<Type> FindDerivedTypes<TBase>()
            where TBase : class
        {
            // Get the assembly containing the type
            Assembly assembly = Assembly.GetAssembly(typeof(TBase));

            // Find all types in the assembly that are subclass of TBase and are not abstract
            var derivedTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(TBase)))
                .ToList();

            return derivedTypes;
        }
    }
}
