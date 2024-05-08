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

namespace PlayEveryWare.EpicOnlineServices.Utility
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using UnityEngine;

    /// <summary>
    /// Contains functions to interact with various json data.
    /// </summary>
    public static class JsonUtility
    {
        /// <summary>
        /// Attempts to parse the given JSON string. If problems are encountered, an error will be logged and the exception will continue.
        /// </summary>
        /// <param name="json">The JSON string to validate.</param>
        private static void ValidateJson(string json)
        {
            try
            {
                var parsedToken = JToken.Parse(json);
            }
            catch (JsonReaderException ex)
            {
                Debug.LogError($"Invalid JSON: {ex.Message}");
#if UNITY_EDITOR
                throw;
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred while attempting to validate JSON: {ex.Message}");
#if UNITY_EDITOR
                throw;
#endif
            }
        }

        /// <summary>
        /// Determines if the given json string is valid json or not. If JSON is invalid, errors will be logged, but exceptions will be suppressed.
        /// </summary>
        /// <param name="json">The JSON string to validate.</param>
        /// <returns>True if json is valid, false otherwise.</returns>
        private static bool IsJsonValid(string json)
        {
            try
            {
                ValidateJson(json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Convert an object into JSON.
        /// </summary>
        /// <param name="obj">The object to serialize into JSON.</param>
        /// <param name="pretty">Whether to make the JSON pretty.</param>
        /// <returns>String representation of the given object serialized.</returns>
        public static string ToJson(object obj, bool pretty = false)
        {
            return JsonConvert.SerializeObject(obj, pretty ? Formatting.Indented : Formatting.None);
        }

        /// <summary>
        /// Return an object deserialized from the given json string. If json is invalid, errors will
        /// be logged, and an object with default values will be returned.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize.</typeparam>
        /// <param name="json">The JSON string.</param>
        /// <returns>The deserialized object.</returns>
        public static T FromJson<T>(string json)
        {
            ValidateJson(json);
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Return an object deserialized from the given json file.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize.</typeparam>
        /// <param name="filepath">The path to the JSON file.</param>
        /// <returns>The deserialized object.</returns>
        public static T FromJsonFile<T>(string filepath)
        {
            string json = FileUtility.ReadAllText(filepath);
            return FromJson<T>(json);
        }

        /// <summary>
        /// Overwrites the given json object with values deserialized from the given json string.
        /// If json is invalid, errors will be logged and no change will be made to the object.
        /// </summary>
        /// <param name="json">The string of json to deserialize values from.</param>
        /// <param name="obj">The object to change the values of.</param>
        public static void FromJsonOverwrite(string json, object obj)
        {
            if (IsJsonValid(json))
            {
                UnityEngine.JsonUtility.FromJsonOverwrite(json, obj);
            }
            else
            {
                Debug.LogWarning($"Object's values were not set using the json given, because the json was invalid.");
            }
        }
    }
}