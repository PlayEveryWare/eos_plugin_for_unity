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

    using JsonUtility = PlayEveryWare.EpicOnlineServices.Utility.JsonUtility;

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
        protected readonly string Filename;
        protected readonly string Directory;

        private string _lastReadJsonString;

        protected Config(string filename) : this(filename, Path.Combine(Application.streamingAssetsPath, "EOS")) { }

        protected Config(string filename, string directory)
        {
            Filename = filename;
            Directory = directory;
        }

        /// <summary>
        /// Retrieves the indicated Config object, reading its values into memory.
        /// </summary>
        /// <typeparam name="T">The Config to retrieve.</typeparam>
        /// <returns>Task<typeparam name="T">Config type.</typeparam></returns>
        public static async Task<T> GetAsync<T>() where T : Config, new()
        {
            T instance = new();
            await instance.ReadAsync();
            return instance;
        }

        /// <summary>
        /// Retrieves the indicated Config object, reading its values into memory.
        /// </summary>
        /// <typeparam name="T">The Config to retrieve.</typeparam>
        /// <returns>Task<typeparam name="T">Config type.</typeparam></returns>
        public static T Get<T>() where T : Config, new()
        {
            T instance = new();
            instance.Read();
            return instance;
        }

        /// <summary>
        /// Returns the fully-qualified path to the file that holds the configuration values.
        /// </summary>
        public string FilePath
        {
            get
            {
                return Path.Combine(Directory, Filename);
            }
        }

        protected virtual async Task ReadAsync()
        {
            bool configFileExists = File.Exists(FilePath);

            // If the file does not already exist, then save it before reading it, a default
            // json will be put in the correct place, and therefore a default set of config
            // values will be loaded.
            if (!configFileExists)
            {
                // This conditional exists because writing a config file is only something
                // that should ever happen in the editor.
#if UNITY_EDITOR
                // If the config file does not currently exist, create it 
                // when it is being read (which is fair to do in the editor)
                await WriteAsync();
#else
                // If the editor is not running, then the config file not
                // existing should throw an error.
                throw new FileNotFoundException($"Config file \"{FilePath}\" does not exist.");
#endif
            }

            using StreamReader reader = new(FilePath);
            _lastReadJsonString = await reader.ReadToEndAsync();
            JsonUtility.FromJsonOverwrite(_lastReadJsonString, this);
        }

        protected virtual void Read()
        {
            bool configFileExists = File.Exists(FilePath);

            if (configFileExists)
            {
                using StreamReader reader = new(FilePath);
                _lastReadJsonString = reader.ReadToEnd();
                JsonUtility.FromJsonOverwrite(_lastReadJsonString, this);
            }
            else
            {
#if UNITY_EDITOR
                Write();
#else
                throw new FileNotFoundException($"Config file \"{FilePath}\" does not exist.");
#endif
            }
        }

        // Functions declared below should only ever be utilized in the editor. They are 
        // so divided to guarantee separation of concerns.
#if UNITY_EDITOR

        /// <summary>
        /// Asynchronously writes the configuration value to file.
        /// </summary>
        /// <param name="prettyPrint">Whether to output "pretty" JSON to the file.</param>
        /// <param name="updateAssetDatabase">Indicates whether to update the asset database after writing.</param>
        /// <returns>Task</returns>
        public virtual async Task WriteAsync(bool prettyPrint = true, bool updateAssetDatabase = true)
        {
            FileInfo configFile = new(FilePath);
            configFile.Directory?.Create();

            await using (StreamWriter writer = new(FilePath))
            {
                var json = JsonUtility.ToJson(this, prettyPrint);
                await writer.WriteAsync(json);
            }

            // If the asset database should be updated, then do the thing.
            //if (updateAssetDatabase)
            //{
            //    await Task.Run(() =>
            //    {
            //        AssetDatabase.SaveAssets();
            //        AssetDatabase.Refresh();
            //    });
            //}
        }

        /// <summary>
        /// Synchronously writes the configuration value to file.
        /// </summary>
        /// <param name="prettyPrint">Whether to output "pretty" JSON to the file.</param>
        /// <param name="updateAssetDatabase">Indicates whether to update the asset database after writing.</param>
        public virtual void Write(bool prettyPrint = true, bool updateAssetDatabase = true)
        {
            FileInfo configFile = new(FilePath);
            configFile.Directory?.Create();

            using StreamWriter writer = new (FilePath);
            var json = JsonUtility.ToJson(this, prettyPrint);
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

            foreach (MemberInfo memberInfo in IteratePropertiesAndFields(configInstance))
            {
                if (GetDefaultValue(memberInfo.MemberType) != memberInfo.MemberValue)
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Returns the default value for a given Type. When the given type is a List of string values,
        /// it will return an empty list of that type (not null). When the given type is string,
        /// it will return an empty string, not null.
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
            // if the given object is null, or is not the same type, then they are
            // not equal.
            if (null == obj || this.GetType() != obj.GetType())
            {
                return false;
            }

            // cast the input object as config
            Config other = obj as Config;

            // get IEnumerable collections for the members of both instances
            IEnumerable<MemberInfo> thisMembers = IteratePropertiesAndFields(this);
            IEnumerable<MemberInfo> otherMembers = IteratePropertiesAndFields(other);

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

        #region Reflection utility functions common to both operations of determining Config equality of default-ness

        /// <summary>
        /// Contains information about either a Property or a Field
        /// </summary>
        private class MemberInfo
        {
            public Type MemberType;
            public object MemberValue;

            public bool Equals(MemberInfo a, MemberInfo b)
            {
                // If the two members are not of the same type, then they can't be equal
                if (a.MemberType != b.MemberType)
                {
                    return false;
                }

                // if the two member values are both null, consider them equal
                if (a.MemberValue == null && null == b.MemberValue)
                {
                    return true;
                }

                // if member info is for a member type that is a value type, then we
                // can directly compare them
                if (a.MemberType.IsValueType)
                {
                    return a.MemberValue == b.MemberValue;
                }

                // otherwise, if the type of the members is a list of strings
                if (a.MemberType == typeof(List<string>))
                {
                    // consider the member values to be equal if one is null and the other is empty
                    return (a.MemberValue == null && (b.MemberValue as List<string>).Count == 0) ||
                           ((a.MemberValue as List<string>).Count == 0 && b.MemberValue == null);
                }
                else if (a.MemberType == typeof(string))
                {
                    // consider the member values to be equal if they are both null and/or empty
                    return (string.IsNullOrEmpty(b.MemberValue as string) &&
                            string.IsNullOrEmpty(a.MemberValue as string));
                }

                // if the member is a reference type, and it's neither a list of strings, nor a string
                // then they can be directly compared.
                return a.MemberValue == b.MemberValue;
            }

            public int GetHashCode(MemberInfo memberInfo)
            {
                return HashCode.Combine(memberInfo.MemberType, memberInfo.MemberValue);
            }
        }

        /// <summary>
        /// Gets an IEnumerable of the type / value pairs for each Field or Property matching the given BindingFlags on the given instance.
        /// </summary>
        /// <typeparam name="T">Type of the given instance.</typeparam>
        /// <param name="instance">Instance to iterate over the Field &amp; Property type / value pairs of.</param>
        /// <param name="bindingAttr">BindingFlags to use when iterating over the Fields and Properties on the instance.</param>
        /// <returns>IEnumerable of the type / value pairs for each Field or Property matching the given BindingFlags on the given instance.</returns>
        private static IEnumerable<MemberInfo> IteratePropertiesAndFields<T>(T instance,
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance)
        {
            // go over the properties
            foreach (PropertyInfo property in typeof(T).GetProperties(bindingAttr))
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