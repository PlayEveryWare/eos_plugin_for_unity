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

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

#if !UNITY_EDITOR
    using UnityEngine.SceneManagement;
#endif

    /// <summary>
    /// SceneUtility is used to interact with scenes in a Unity Project.
    /// </summary>
    public static class SceneUtility
    {
        /// <summary>
        /// Determines if the given scene name is in fact a scene that is built in the project.
        /// </summary>
        /// <param name="sceneName">The name of the scene to check.</param>
        /// <returns>True if the scene name given refers to a scene that exists in the project, false otherwise.</returns>
        public static bool IsValidSceneName(string sceneName)
        {
            // When running in the editor, the SceneManager is a little weird, so 
            // if we are in the editor we check to see if the scene is valid a different
            // way.
            Debug.Log($"Determining if \"{sceneName}\" is a valid scene name.");
#if UNITY_EDITOR
            return EditorBuildSettings.scenes
                .Any(scene => sceneName == Path.GetFileNameWithoutExtension(scene.path));
#else
            foreach(string name in GetSceneNames())
            {
                Debug.Log($"SceneName: \"{name}\".");
                if (sceneName == name)
                    return true;
            }

            return false;
#endif
        }

        public static IEnumerable<string> GetSceneNames()
        {
#if UNITY_EDITOR
            return EditorBuildSettings.scenes
                .Select(scene => Path.GetFileNameWithoutExtension(scene.path));
#else
            Debug.Log($"Scene count: {SceneManager.sceneCount}");
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                var scene = SceneManager.GetSceneByBuildIndex(i);
                yield return Path.GetFileNameWithoutExtension(scene.path);
            }
#endif
        }
    }
}