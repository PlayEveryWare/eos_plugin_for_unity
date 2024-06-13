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
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;
    using System.Reflection;   
    using System.IO;
    using System.Text;
    using JsonUtility = PlayEveryWare.EpicOnlineServices.Utility.JsonUtility;
    using System.Runtime.CompilerServices;
    using Utility;

    /// <summary>
    /// Represents a set of configuration data for use by the EOS Plugin for
    /// Unity
    /// </summary>
    [Serializable]
    public abstract class Config
#if UNITY_EDITOR
        : ICloneable
#endif
    {
#if !UNITY_EDITOR
        // NOTE: This caching mechanism works because it is expected that there
        //       is only ever one Config per type. If ever there is a scenario
        //       where there are multiple config files for a single type, this
        //       will need to be changed. This is not a likely scenario.
        protected static IDictionary<Type, Config> s_cachedConfigs = 
            new Dictionary<Type, Config>();
#endif



        /// <summary>
        /// Contains a registration that maps config type to the constructor, to
        /// enforce usage of the factor pattern for classes that derive from the
        /// Config class.
        /// </summary>
        private static Dictionary<Type, Func<Config>> s_factories = 
            new Dictionary<Type, Func<Config>>();

        /// <summary>
        /// The name of the file that contains the config values.
        /// </summary>
        protected readonly string Filename;

        /// <summary>
        /// The directory that contains the file.
        /// </summary>
        protected readonly string Directory;

        /// <summary>
        /// The contents of the JSON file the last time it was read.
        /// </summary>
        private string _lastReadJsonString;

        /// <summary>
        /// Instantiate a new config based on the file at the given filename -
        /// in a default directory.
        /// </summary>
        /// <param name="filename">
        /// The name of the file containing the config values.
        /// </param>
        protected Config(string filename) : 
            this(filename, Path.Combine(
                Application.streamingAssetsPath, "EOS")) { }

        /// <summary>
        /// Instantiates a new config based on the file at the given file and
        /// directory.
        /// </summary>
        /// <param name="filename">
        /// The name of the file containing the config values.
        /// </param>
        /// <param name="directory">
        /// The directory that contains the file.
        /// </param>
        protected Config(string filename, string directory)
        {
            Filename = filename;
            Directory = directory;
        }

        /// <summary>
        /// Allows deriving classes to register their constructor method in
        /// order to enforce the factory pattern. This requires that each class
        /// that derives from Config must implement a static method registering
        /// its constructor.
        /// </summary>
        /// <typeparam name="T">The config type.</typeparam>
        /// <param name="factory">The function to create the config type</param>
        protected static void RegisterFactory<T>(Func<T> factory) 
            where T : Config
        {
            s_factories[typeof(T)] = factory;
        }

        /// <summary>
        /// Try to retrieve the factory method for the indicated type that can
        /// be used to create a new instance of the given config type.
        /// </summary>
        /// <typeparam name="T">
        /// The type of config to get the factory method for.
        /// </typeparam>
        /// <param name="factory">
        /// The factory method that instantiates the config indicated.
        /// </param>
        /// <returns>
        /// True if the factory is registered, false otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the indicated type does not have a corresponding constructor
        /// function registered, a verbose exception will be thrown indicating
        /// how to properly implement the Config implementing class such that
        /// its constructor is properly registered.
        /// </exception>
        private static bool TryGetFactory<T>(out Func<Config> factory) 
            where T : Config
        {
            // Ensure static constructor of template variable type is called
            RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);

            if(!s_factories.TryGetValue(typeof(T), out factory))
            {
                throw new InvalidOperationException(
                    $"No factory method has been registered for " +
                    $"type \"{typeof(T).FullName}\". " +
                    $"Please make sure that \"{typeof(T).FullName}\" " +
                    $"registers its constructor with the base Config class " +
                    $"via a static constructor.");
            }

            return true;
        }

        /// <summary>
        /// Retrieves indicated Config object, reading its values into memory.
        /// </summary>
        /// <typeparam name="T">The Config to retrieve.</typeparam>
        /// <returns>Task<typeparam name="T">Config type.</typeparam></returns>
        public static async Task<T> GetAsync<T>() where T : Config
        {
            // NOTE: This block (and the corresponding one below) exists so that
            //       the config values are only cached when not in the editor.
            //       In the editor, config files can be changed, so they should
            //       not be cached.
#if !UNITY_EDITOR
            // Return cached copy if it exists.
            if (s_cachedConfigs.TryGetValue(typeof(T), out Config config))
            {
                return (T)config;
            }
#endif
            // Try to get the factory method used to instantiate the config.
            TryGetFactory<T>(out Func<Config> factory);

            // Use the factory method to create the config.
            T instance = (T)factory();

            // Asynchronously read config values from the corresponding file.
            await instance.ReadAsync();

#if !UNITY_EDITOR
            // Cache the newly created config with its values having been read.
            s_cachedConfigs.Add(typeof(T), instance);
#endif

            // Return the config being retrieved.
            return instance;
        }

        /// <summary>
        /// Retrieves indicated Config object, reading its values into memory.
        /// </summary>
        /// <typeparam name="T">The Config to retrieve.</typeparam>
        /// <returns>Task<typeparam name="T">Config type.</typeparam></returns>
        public static T Get<T>() where T : Config
        {
            // NOTE: This block (and the corresponding one below) exists so that
            //       the config values are only cached when not in the editor.
            //       In the editor, config files can be changed, so they should
            //       not be cached.
#if !UNITY_EDITOR
            // Return cached copy if it exists.
            if (s_cachedConfigs.TryGetValue(typeof(T), out Config config))
            {
                return (T)config;
            }
#endif
            // Try to get the factory method used to instantiate the config.
            TryGetFactory<T>(out Func<Config> factory);

            // Use the factory method to create the config.
            T instance = (T)factory();

            // Read the config values from the corresponding file.
            instance.Read();

#if !UNITY_EDITOR
            // Cache the newly created config with its values having been read.
            s_cachedConfigs.Add(typeof(T), instance);
#endif

            // Return the config being retrieved.
            return instance;
        }

        /// <summary>
        /// Returns the fully-qualified path to the file that holds the
        /// configuration values.
        /// </summary>
        public string FilePath
        {
            get
            {
                return Path.Combine(Directory, Filename);
            }
        }

        /// <summary>
        /// Asynchronously read the values from the JSON file associated with
        /// this Config
        /// </summary>
        /// <returns>Task.</returns>
        protected virtual async Task ReadAsync()
        {
            bool configFileExists = File.Exists(FilePath);

            // If the file does not already exist, then save it before reading
            // it, a default json will be put in the correct place, and
            // therefore a default set of config values will be loaded.
            if (!configFileExists)
            {
                // This conditional exists because writing a config file is only
                // something that should ever happen in the editor.
#if UNITY_EDITOR
                // If the config file does not currently exist, create it 
                // when it is being read (which is fair to do in the editor)
                await WriteAsync();
#else
                // If the editor is not running, then the config file not
                // existing should throw an error.
                throw new FileNotFoundException(
                    $"Config file \"{FilePath}\" does not exist.");
#endif
            }

            _lastReadJsonString = await FileUtility.ReadAllTextAsync(FilePath);
            JsonUtility.FromJsonOverwrite(_lastReadJsonString, this);
        }

        /// <summary>
        /// Synchronously reads the contents of a Config from the json file that
        /// defines it.
        /// </summary>
        protected virtual void Read()
        {
            bool configFileExists = File.Exists(FilePath);

            if (configFileExists)
            {
                _lastReadJsonString = FileUtility.ReadAllText(FilePath);
                JsonUtility.FromJsonOverwrite(_lastReadJsonString, this);
            }
            else
            {
#if UNITY_EDITOR
                Write();
#else
                throw new FileNotFoundException(
                    $"Config file \"{FilePath}\" does not exist.");
#endif
            }
        }

        // Functions declared below should only ever be utilized in the editor.
        // They are so divided to guarantee separation of concerns.
#if UNITY_EDITOR

        /// <summary>
        /// Asynchronously writes the configuration value to file.
        /// </summary>
        /// <param name="prettyPrint">
        /// Whether to output "pretty" JSON to the file.
        /// </param>
        /// <param name="updateAssetDatabase">
        /// Indicates whether to update the asset database after writing.
        /// </param>
        /// <returns>Task</returns>
        public virtual async Task WriteAsync(
            bool prettyPrint = true, 
            bool updateAssetDatabase = true)
        {
            FileInfo configFile = new(FilePath);
            configFile.Directory?.Create();

            var json = JsonUtility.ToJson(this, prettyPrint);

            // If the json hasn't changed since it was last read, then
            // take no action.
            if (json == _lastReadJsonString)
                return;

            await using StreamWriter writer = new(FilePath);
            await writer.WriteAsync(json);
        }

        /// <summary>
        /// Synchronously writes the configuration value to file.
        /// </summary>
        /// <param name="prettyPrint">
        /// Whether to output "pretty" JSON to the file.
        /// </param>
        /// <param name="updateAssetDatabase">
        /// Indicates whether to update the asset database after writing.
        /// </param>
        public virtual void Write(
            bool prettyPrint = true, 
            bool updateAssetDatabase = true)
        {
            FileInfo configFile = new(FilePath);
            configFile.Directory?.Create();

            var json = JsonUtility.ToJson(this, prettyPrint);

            // If the json hasn't changed since it was last read, then
            // take no action.
            if (json == _lastReadJsonString)
                return;

            using StreamWriter writer = new(FilePath);
            writer.Write(json);

            if (updateAssetDatabase)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }


        /// <summary>
        /// Determines whether the values in the Config have their
        /// default values
        /// </summary>
        /// <returns>
        /// True if the Config has its default values, false otherwise.
        /// </returns>
        public bool IsDefault()
        {
            return IsDefault(this);
        }


        /// <summary>
        /// Returns member-wise clone of configuration data
        /// (copies the values).
        /// </summary>
        /// <returns>A copy of the configuration data.</returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #region Functions to help determine if a config has default values

        /// <summary>
        /// Determines if the given Config implementation instance represents a
        /// config that has default values only.
        /// </summary>
        /// <typeparam name="T">Type of Config being inspected.</typeparam>
        /// <param name="configInstance">The Config instance to check.</param>
        /// <returns>True if Config has default-only values.</returns>
        private static bool IsDefault<T>(T configInstance) where T : Config
        {
            /*
             * Thought Process behind common function:
             *
             * Because all implementations of Config are Serializable, the
             * values in them which will ultimately be written to a JSON file
             * are by necessity either public properties or public fields.
             * These fields can be inspected via reflection (which is okay
             * because this only ever happens in the editor) to determine
             * whether or not they have a value other than the default.
             *
             * One tweak that is made to a straight-forward implementation is
             * that in the case of a List being inspected, it is considered
             * to be default not only if it is null, but also if the list is
             * empty. This is also true for strings, since they can be null.
             * This tweak is made, and documented within the function
             * GetDefaultValue.
             *
             */

            return IteratePropertiesAndFields(configInstance)
                .All(mInfo => 
                    GetDefaultValue(mInfo.MemberType) == mInfo.MemberValue);
        }


        /// <summary>
        /// Returns the default value for a given Type. When the given type is a
        /// List of string values, it will return an empty list of that type
        /// (not null). When the given type is string, it will return an empty
        /// string, not null.
        /// </summary>
        /// <param name="type">The type to get the default value of.</param>
        /// <returns>The default value for the Type indicated.</returns>
        private static object GetDefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            else
            {
                // For the purpose of determining default values, for reference
                // types of List<string> or string they also need to be checked
                // for if they are empty.
                if (type == typeof(List<string>))
                {
                    return new List<string>();
                }
                else if (type == typeof(string))
                {
                    return "";
                }

                return null;
            }
        }

        #endregion

        #region Equality operators

        public override bool Equals(object obj)
        {
            // if the given object is null, or is not the same type, then they
            // are not equal.
            if (null == obj || this.GetType() != obj.GetType())
            {
                return false;
            }

            // cast the input object as config
            Config other = obj as Config;

            // get IEnumerable collections for the members of both instances
            var thisMembers = IteratePropertiesAndFields(this);
            var otherMembers = IteratePropertiesAndFields(other);

            // Use linq to determine if the sequences are equal
            return thisMembers.SequenceEqual(otherMembers);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IteratePropertiesAndFields(this));
        }

        public static bool operator ==(Config left, Config right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(Config left, Config right)
        {
            return !(left == right);
        }

        #endregion

        #region Reflection utility functions

        /// <summary>
        /// Contains information about either a Property or a Field
        /// </summary>
        private class MemberInfo
        {
            public Type MemberType;
            public object MemberValue;

            public bool Equals(MemberInfo a, MemberInfo b)
            {
                // If the two members are not of the same type, then they can't
                // be equal
                if (a.MemberType != b.MemberType)
                {
                    return false;
                }

                // if the two member values are both null, consider them equal
                if (a.MemberValue == null && null == b.MemberValue)
                {
                    return true;
                }

                // if member info is for a member type that is a value type,
                // then we
                // can directly compare them
                if (a.MemberType.IsValueType)
                {
                    return a.MemberValue == b.MemberValue;
                }

                // otherwise, if the type of the members is a list of strings
                if (a.MemberType == typeof(List<string>))
                {
                    // consider the member values to be equal if one is null and
                    // the other is empty
                    return ((a.MemberValue == null && 
                             ((List<string>)b.MemberValue).Count == 0) 
                            ||
                            (((List<string>)a.MemberValue).Count == 0 && 
                             b.MemberValue == null));
                }
                else if (a.MemberType == typeof(string))
                {
                    // consider the member values to be equal if they are both
                    // null and/or empty
                    return (string.IsNullOrEmpty(b.MemberValue as string) &&
                            string.IsNullOrEmpty(a.MemberValue as string));
                }

                // if the member is a reference type, and it's neither a list of
                // strings, nor a string then they can be directly compared.
                return a.MemberValue == b.MemberValue;
            }

            public int GetHashCode(MemberInfo memberInfo)
            {
                return HashCode.Combine(
                    memberInfo.MemberType, 
                    memberInfo.MemberValue);
            }
        }

        /// <summary>
        /// Gets an IEnumerable of the type / value pairs for each Field or
        /// Property matching the given BindingFlags on the given instance.
        /// </summary>
        /// <typeparam name="T">Type of the given instance.</typeparam>
        /// <param name="instance">
        /// Instance to iterate over the Field &amp; Property type / value pairs
        /// of.
        /// </param>
        /// <param name="bindingAttr">
        /// BindingFlags to use when iterating over the Fields and Properties on
        /// the instance.
        /// </param>
        /// <returns>
        /// IEnumerable of the type / value pairs for each Field or Property
        /// matching the given BindingFlags on the given instance.
        /// </returns>
        private static IEnumerable<MemberInfo> IteratePropertiesAndFields<T>(
            T instance,
            BindingFlags bindingAttr = 
                BindingFlags.Public | BindingFlags.Instance)
        {
            // go over the properties
            foreach (var property in typeof(T).GetProperties(bindingAttr))
            {
                // make use of yield to return into an IEnumerable
                yield return new MemberInfo()
                {
                    MemberType = property.PropertyType,
                    MemberValue = property.GetValue(instance)
                };
            }

            // go over the fields
            foreach (FieldInfo field in typeof(T).GetFields(bindingAttr))
            {
                // make use of yield to return into an IEnumerable
                yield return new MemberInfo()
                {
                    MemberType = field.FieldType,
                    MemberValue = field.GetValue(instance)
                };
            }
        }

        #endregion

#endif
    }

}