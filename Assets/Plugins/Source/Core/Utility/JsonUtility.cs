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
    using UnityEngine;

    /// <summary>
    /// Contains functions to interact with various json data.
    /// </summary>
    public static class JsonUtility
    {
        /// <summary>
        /// Attempts to use UnityEngine's JsonUtility to deserialize an object 
        /// from JSON.
        /// </summary>
        /// <typeparam name="T">
        /// The type of object to deserialize into.
        /// </typeparam>
        /// <param name="json">The JSON to deserialize from.</param>
        /// <param name="obj">
        /// The object to assign the deserialized value to.
        /// </param>
        /// <returns>
        /// True if the deserialization was successful, false otherwise (which
        /// usually will be an indicator of invalid JSON).
        /// </returns>
        private static bool TryFromJson<T>(string json, out T obj)
        {
            try
            {
                obj = UnityEngine.JsonUtility.FromJson<T>(json);
                return true;
            }
            catch (System.ArgumentException)
            {
                Debug.LogError($"Invalid JSON: \"{json}\".");
                throw;
            }

            return false;
        }

        /// <summary>
        /// Convert an object into JSON.
        /// </summary>
        /// <param name="obj">The object to serialize into JSON.</param>
        /// <param name="pretty">Whether to make the JSON pretty.</param>
        /// <returns>
        /// String representation of the given object serialized.
        /// </returns>
        public static string ToJson(object obj, bool pretty = false)
        {
            return UnityEngine.JsonUtility.ToJson(obj, pretty);
        }

        /// <summary>
        /// Return an object deserialized from the given json string. If json is
        /// invalid, errors will be logged, and an object with default values 
        /// will be returned.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize.</typeparam>
        /// <param name="json">The JSON string.</param>
        /// <returns>The deserialized object.</returns>
        public static T FromJson<T>(string json)
        {
            // The return value from this method is not checked because the
            // purpose of calling it is to cause an exception to be thrown if
            // the Json is invalid.
            TryFromJson(json, out T obj);

            return obj;
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
        /// Overwrites the given json object with values deserialized from the
        /// given json string. If json is invalid, errors will be logged and no
        /// change will be made to the object.
        /// </summary>
        /// <param name="json">
        /// The string of json to deserialize values from.
        /// </param>
        /// <param name="obj">The object to change the values of.</param>
        public static void FromJsonOverwrite<T>(string json, T obj)
        {
            // NOTE: This compiler conditional exists so that checking the
            //       validity of json in this particular method only happens if
            //       in editor mode. During editor builds, the json is
            //       deserialized twice (the first time to check validity).
#if UNITY_EDITOR
            // Neither the return value, nor the out value are checked when the
            // following method is called, because the purpose of calling it is
            // simply to throw an exception if the Json is invalid. Note that
            // this does cause the Json to be deserialized twice, but only in
            // editor builds.
            TryFromJson(json, out T discardObj);
#endif

            UnityEngine.JsonUtility.FromJsonOverwrite(json, obj);
        }
    }
}