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

namespace PlayEveryWare.EpicOnlineServices.Tests.Config
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Config = EpicOnlineServices.Config;

    public class ConfigTests
    {
        /// <summary>
        /// Finds all non-abstract derived classes of a specified type within
        /// the same assembly. Eventually, this particular component can be put
        /// into a class containing generic reflection utility functions,
        /// because currently ConfigTests is the only place this utility method
        /// is used, it is kept within the ConfigTests class until it's needed
        /// elsewhere.
        /// </summary>
        /// <typeparam name="TBase">
        /// The type to find derived classes for.
        /// </typeparam>
        /// <returns>
        /// An enumerable of types that are non-abstract and derived from the
        /// specified base type.
        /// </returns>
        private static IEnumerable<Type> FindDerivedTypes<TBase>()
            where TBase : class
        {
            // Get the assembly containing the type.
            Assembly assembly = Assembly.GetAssembly(typeof(TBase));

            // Find all types in the assembly that are subclass of TBase and are
            // not abstract.
            var derivedTypes = assembly.GetTypes()
                .Where(t => t.IsClass &&
                            !t.IsAbstract &&
                            t.IsSubclassOf(typeof(TBase)));

            return derivedTypes;
        }

        /// <summary>
        /// Due to the way that the Config base class requires registration of
        /// the config type with the constructor for that type in-order to
        /// accomplish the factory pattern, it is necessary to check that the
        /// type requested equals the type that the factory method generates.
        /// </summary>
        [Test]
        public static void Get_ConfigDerivedClass_Valid()
        {
            // Iterate through the types that derive from the base abstract
            // Config class.
            foreach (var type in FindDerivedTypes<Config>())
            {
                // Get the "generic" version of the get method (factory method
                // used to retrieve a Config of a certain type).
                var getMethod = typeof(Config).GetMethod("Get");

                // Do not continue if the factory method could not be created.
                if (null == getMethod)
                    continue;

                // Create the typed version of the factory method.
                var typedGetMethod = getMethod.MakeGenericMethod(type);

                // Obtain a config object using the factory method.
                var config = typedGetMethod.Invoke(
                    null, 
                    null);

                // Make certain that the factory method generates the expected
                // type.
                Assert.AreEqual(config.GetType(), type);
            }
        }
    }
}
