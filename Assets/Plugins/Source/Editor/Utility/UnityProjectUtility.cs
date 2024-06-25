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

// Uncomment the following define to enable menu items detailed below
//#define PROJECT_UTILITY_ENABLED

#if PROJECT_UTILITY_ENABLED

namespace PlayEveryWare.EpicOnlineServices.Editor.Utility
{
    using System.IO;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine.SceneManagement;
    using System.Collections.Generic;

    public class UnityProjectUtility
    {
        [MenuItem("Tools/Find Missing Scripts/In Prefabs")]
        public static void FindAllMissingScriptsInPrefabs()
        {
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            int missingScriptCount = 0;

            foreach (string assetPath in allAssetPaths)
            {
                // Skip if the asset is not a prefab
                if (!assetPath.EndsWith(".prefab"))
                    continue;

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                Component[] components = prefab.GetComponentsInChildren<Component>(true);
                foreach (Component component in components)
                {
                    // skip if the component is not null (we're looking for null)
                    if (component != null)
                        continue;

                    Debug.LogError("Missing script found in prefab: " + assetPath, prefab);
                    missingScriptCount++;
                    break; // Once a missing script is found, no need to check further.
                }
            }

            if (missingScriptCount == 0)
            {
                Debug.Log("No missing scripts found in any prefabs.");
            }
            else
            {
                Debug.LogWarning($"{missingScriptCount} prefabs with missing scripts found.");
            }
        }

        [MenuItem("Tools/Find Missing Scripts/In Prefabs", isValidateFunction: true)]
        public static bool CanFindMissingScriptsInPrefabs()
        {
            return !EditorApplication.isPlaying;
        }

        [MenuItem("Tools/Find Missing Scripts/In Scenes")]
        private static void FindMissingScriptsInScenes()
        {
            // Store the currently open scene
            var previouslyOpenScene = SceneManager.GetActiveScene().path;

            IList<(string message, Object context)> missingScriptMessages = new List<(string message, Object context)>();

            // Iterate over all the scenes
            foreach (string scenePath in EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes))
            {
                // Open the scene
                Scene currentScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                // Get all the game objects (including inactive objects)
                GameObject[] gameObjects = Object.FindObjectsOfType<GameObject>(true);

                // Iterate over all the game objects
                foreach (GameObject go in gameObjects)
                {
                    // Get all the components
                    Component[] components = go.GetComponents<Component>();
                    foreach (Component component in components)
                    {
                        // Skip if the component is there.
                        if (component != null) continue;

                        missingScriptMessages.Add(($"Script \"{FullGameObjectPath(go)}\" in scene \"{Path.GetFileNameWithoutExtension(scenePath)}\" is missing.", go));
                    }
                }
            }

            if (0 == missingScriptMessages.Count)
            {
                Debug.Log("There were no scripts found to be missing in any scene.");
            }
            else
            {
                Debug.LogWarning($"There were {missingScriptMessages.Count} scripts found to be missing in scenes.");
                foreach ((string message, Object context) in missingScriptMessages)
                {
                    Debug.LogWarning(message, context);
                }
            }

            // Reopen the scene that was open originally
            EditorSceneManager.OpenScene(previouslyOpenScene, OpenSceneMode.Single);
        }

        [MenuItem("Tools/Find Missing Scripts/In Scenes", isValidateFunction: true)]
        private static bool CanFindMissingScriptsInScenes()
        {
            // Scenes can only be searched if not in play mode.
            return !EditorApplication.isPlaying;
        }

        [MenuItem("Tools/Find Unreferenced Prefabs")]
        public static void FindAllUnreferencedPrefabs()
        {
            HashSet<string> referencedPrefabGuids = new();
            HashSet<string> checkedPrefabGuids = new();

            // Get the path to the currently open scene so we can reopen it
            // when we are done looking for unreferenced prefabs.
            var previouslyOpenScene = SceneManager.GetActiveScene().path;

            // First pass: Find prefabs referenced in scenes
            foreach (var scenePath in EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes))
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                FindPrefabsInCurrentScene(ref referencedPrefabGuids);
            }

            EditorSceneManager.OpenScene(previouslyOpenScene, OpenSceneMode.Single);

            Debug.Log($"There are a total of {referencedPrefabGuids.Count} prefabs directly referenced by scenes.");

            // Second pass: Find prefabs referenced by other prefabs
            Debug.Log($"Now checking each prefab for prefab dependencies.");
            var prefabsReferencedByScenesDirectory = new HashSet<string>(referencedPrefabGuids);
            foreach (var prefabGuid in prefabsReferencedByScenesDirectory)
            {
                FindPrefabDependencies(ref checkedPrefabGuids, ref referencedPrefabGuids, prefabGuid);
            }

            // Compare to list of all prefabs
            string[] allPrefabGuids = AssetDatabase.FindAssets("t:Prefab");
            Debug.Log(
                $"Comparing discovered (referenced) prefabs ({referencedPrefabGuids.Count}) to all prefabs ({allPrefabGuids.Length}).");
            foreach (var prefabGuid in allPrefabGuids)
            {
                // Skip if the prefab is referenced
                if (referencedPrefabGuids.Contains(prefabGuid))
                    continue;

                string path = AssetDatabase.GUIDToAssetPath(prefabGuid);
                Debug.LogWarning($"Unreferenced Prefab: {path}");
            }
        }

        [MenuItem("Tools/Find Unreferenced Prefabs", isValidateFunction: true)]
        public static bool CanFindUnreferencedPrefabs()
        {
            return !EditorApplication.isPlaying;
        }

        private static void FindPrefabsInCurrentScene(ref HashSet<string> referencedPrefabGuids)
        {
            GameObject[] allGameObjects = Object.FindObjectsOfType<GameObject>(true);
            foreach (var go in allGameObjects)
            {
                AddPrefabReferencesFromGameObject(ref referencedPrefabGuids, go);
            }
        }

        private static void AddPrefabReferencesFromGameObject(ref HashSet<string> referencedPrefabGuids, GameObject go)
        {
            var components = go.GetComponents<Component>();
            foreach (var component in components)
            {
                if (component == null) continue;

                SerializedObject so = new(component);
                SerializedProperty sp = so.GetIterator();

                while (sp.Next(true))
                {
                    if (sp.propertyType != SerializedPropertyType.ObjectReference || sp.objectReferenceValue == null)
                    {
                        continue;
                    }

                    string path = AssetDatabase.GetAssetPath(sp.objectReferenceValue);

                    // Skip if the path is null, or if it is not a prefab.
                    if (string.IsNullOrEmpty(path) || !path.EndsWith(".prefab")) continue;

                    string guid = AssetDatabase.AssetPathToGUID(path);
                    referencedPrefabGuids.Add(guid);
                }
            }
        }

        private static void FindPrefabDependencies(ref HashSet<string> checkedPrefabGuids,
            ref HashSet<string> referencedPrefabGuids, string prefabGuid)
        {
            if (checkedPrefabGuids.Contains(prefabGuid)) return; // Avoid circular references or rechecking prefabs

            checkedPrefabGuids.Add(prefabGuid);

            string path = AssetDatabase.GUIDToAssetPath(prefabGuid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                return;
            }

            AddPrefabReferencesFromGameObject(ref referencedPrefabGuids, prefab);

            // Check if this prefab references other prefabs and add them to the list
            Component[] components = prefab.GetComponentsInChildren<Component>(true);
            foreach (Component component in components)
            {
                // skip if the component is null
                if (null == component) continue;

                SerializedObject so = new SerializedObject(component);
                SerializedProperty sp = so.GetIterator();
                while (sp.Next(true))
                {
                    if (sp.propertyType != SerializedPropertyType.ObjectReference || sp.objectReferenceValue == null)
                    {
                        continue;
                    }

                    string depPath = AssetDatabase.GetAssetPath(sp.objectReferenceValue);
                    if (string.IsNullOrEmpty(depPath) || !depPath.EndsWith(".prefab")) continue;

                    string depGuid = AssetDatabase.AssetPathToGUID(depPath);
                    if (referencedPrefabGuids.Contains(depGuid))
                    {
                        continue;
                    }

                    referencedPrefabGuids.Add(depGuid);
                    FindPrefabDependencies(ref checkedPrefabGuids, ref referencedPrefabGuids,
                        depGuid); // Recursively check this new prefab
                }
            }
        }

        

        /// <summary>
        /// Determine the full path of a given GameObject recursively.
        /// </summary>
        /// <param name="go">The game object to obtain the path of.</param>
        /// <returns>A string path indicating the path of the GameObject.</returns>
        private static string FullGameObjectPath(GameObject go)
        {
            return go.transform.parent == null
                ? go.name
                : FullGameObjectPath(go.transform.parent.gameObject) + "/" + go.name;
        }
    }

}
#endif