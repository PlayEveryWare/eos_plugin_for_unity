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

namespace PlayEveryWare.EpicOnlineServices.Editor.Utility
{
    using System.Collections.Generic;
    using System;
    using System.Linq;
    using System.Reflection;

    public static class ReflectionUtility
    {
        public static List<object> CreateInstancesOfDerivedGenericClasses(Type genericBaseType)
        {
            if (!genericBaseType.IsGenericTypeDefinition || !genericBaseType.IsAbstract)
            {
                throw new ArgumentException("The provided type must be a generic abstract class.");
            }

            // Get the assembly that defines the generic base type
            var assembly = Assembly.GetExecutingAssembly();

            // Find all concrete classes deriving from any closed type of the generic base type
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.BaseType is { IsGenericType: true }
                            && t.BaseType.GetGenericTypeDefinition() == genericBaseType)
                .ToList();

            var instances = new List<object>();

            foreach (var type in types)
            {
                // Ensure there is a parameter-less constructor
                var constructorInfo = type.GetConstructor(Type.EmptyTypes);
                if (constructorInfo != null)
                {
                    // Create an instance and add it to the list
                    var instance = Activator.CreateInstance(type);
                    instances.Add(instance);
                }
                else
                {
                    throw new InvalidOperationException($"The type {type.Name} does not have a parameter-less constructor.");
                }
            }

            return instances;
        }

    }
}