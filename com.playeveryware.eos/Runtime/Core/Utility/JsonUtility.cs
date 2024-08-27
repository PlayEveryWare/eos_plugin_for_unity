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

namespace PlayEveryWare.EpicOnlineServices.Utility
{
    using UnityEngine;

    /// <summary>
    /// Contains functions to interact with various json data.
    /// </summary>
    public static class JsonUtility
    {
        /// <summary>
        /// Tries to parse the given JSON into an object.
        /// </summary>
        /// <typeparam name="T">The type to deserialize from the given json.
        /// </typeparam>
        /// <param name="json">
        /// The JSON to deserialize from.
        /// </param>
        /// <param name="obj">The object to contain the deserialized value.
        /// </param>
        /// <returns>
        /// True if the json was successfully deserialized, false otherwise.
        /// </returns>
        private static bool TryFromJson<T>(string json, out T obj)
        {
            obj = default;
            try
            {
                obj = UnityEngine.JsonUtility.FromJson<T>(json);
                return true;
            }
            catch
            {
                Debug.LogError($"Unable to parse object of type " +
                               $"\"{typeof(T).FullName}\" from " +
                               $"JSON: \"{json}\".");
#if UNITY_EDITOR
                // If running in the context of the Unity editor, then throw the
                // exception in an effort to "fail loudly" so that the user is
                // more likely to fix the problem. If not in editor mode, still
                // log the error, but do not throw an exception.
                throw;
#else
                // The else conditional is here because if an exception is not
                // thrown, a value still needs to be returned.
                return false;
#endif
            }
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
        /// will be returned depending on whether running in editor mode or as a
        /// standalone.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize.</typeparam>
        /// <param name="json">The JSON string.</param>
        /// <returns>The deserialized object.</returns>
        public static T FromJson<T>(string json)
        {
            // NOTE: The return value of the TryFromJson method call below is
            //       not observed, because regardless of if the from json method
            //       was successful, the value defined by the out variable is
            //       returned to the caller.
            _ = TryFromJson(json, out T returnObject);

            return returnObject;
        }

        /// <summary>
        /// Return an object deserialized from the given json file.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize.</typeparam>
        /// <param name="filepath">The path to the JSON file.</param>
        /// <returns>The deserialized object.</returns>
        public static T FromJsonFile<T>(string filepath)
        {
            string json = FileSystemUtility.ReadAllText(filepath);
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
        /// <param name="obj">
        /// The object to change the values of.
        /// </param>
        public static void FromJsonOverwrite<T>(string json, T obj)
        {
            // NOTES:
            //
            // If the type 'T' indicated is abstract, then it is not possible to
            // use UnityEngine.JsonUtility to verify the validity of the
            // incoming JSON. This is because the validation is accomplished by
            // performing the deserialization step and catching any thrown
            // exceptions. Therefore, if the type indicated is abstract, don't
            // bother trying to validate, go straight to using the
            // 'FromJsonOverwrite' function in UnityEngine.JsonUtility.
            //
            // If the type indicated is _not_ abstract, then it is possible to
            // utilize the validation approach, therefore in that scenario
            // perform validation before performing the intended operation.
            //
            // That this code can only validate JSON representing a non-abstract
            // class is a limitation of the UnityEngine.JsonUtility. Most other
            // available libraries will support this and remove the need for
            // this check.
            if (typeof(T).IsAbstract || TryFromJson(json, out T _))
            {
                UnityEngine.JsonUtility.FromJsonOverwrite(json, obj);
            }
        }
    }
}