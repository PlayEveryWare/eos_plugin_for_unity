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

// NOTE: Uncomment the following define to enable menu items in the Unity Editor
//       that are implemented below.
// #define PROJECT_UTILITY_ENABLED

#if PROJECT_UTILITY_ENABLED

namespace PlayEveryWare.EpicOnlineServices.Editor.Utility
{
    using System.IO;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine.SceneManagement;
    using System.Collections.Generic;
    using System.Linq;

    #region Extension Methods

    /*
     * NOTE: In the typical course of a project, a single class per file policy is good to
     *       adhere to, however in context with the scope of the project as one that is
     *       not centered on extending Unity, the decision was made to keep these within the
     *       current file. If these focuses or scopes change, it might make sense to break
     *       these extension classes off.
     */

    /// <summary>
    /// Handles a variety of extensions to GameObjects to make it easier to follow the logic of
    /// how they are being inspected.
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Retrieves a unique list of GUIDs for each prefab that is attached as a component to the given GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to find all the prefab GUIDs of.</param>
        /// <returns>A unique set of GUIDs for each prefab attached to given GameObject.</returns>
        public static HashSet<string> GetPrefabGuids(this GameObject gameObject)
        {
            HashSet<string> guids = new();
            var components = gameObject.GetComponents<Component>();
            foreach (var component in components)
            {
                if (component == null) continue;

                SerializedObject so = new(component);
                SerializedProperty sp = so.GetIterator();

                while (sp.Next(true))
                {
                    // If the SerializedProperty is either not an ObjectReference, or if the value of that reference is null, then continue
                    if (sp.propertyType != SerializedPropertyType.ObjectReference || sp.objectReferenceValue == null)
                    {
                        continue;
                    }

                    string path = AssetDatabase.GetAssetPath(sp.objectReferenceValue);

                    // Skip if the path is null, or if it is not a prefab.
                    if (string.IsNullOrEmpty(path) || !path.EndsWith(".prefab")) continue;

                    string guid = AssetDatabase.AssetPathToGUID(path);
                    guids.Add(guid);
                }
            }

            return guids;
        }

        public static string GetFullPath(this GameObject gameObject)
        {
            return gameObject.transform.parent == null
                ? gameObject.name
                : gameObject.transform.parent.gameObject.GetFullPath() + "/" + gameObject.name;
        }
    }
    
    #endregion

    /// <summary>
    /// Provides utility functions for inspecting a Unity project.
    /// </summary>
    public class UnityProjectUtility
    {
        #region Methods to find missing scripts

        [MenuItem("Tools/Find Missing Scripts/In Prefabs")]
        public static void FindAllMissingScriptsInPrefabs()
        {
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            int missingScriptCount = 0;

            foreach (string assetPath in allAssetPaths)
            {
                // If the asset isn't a prefab, then continue
                if (!assetPath.EndsWith(".prefab")) { continue; }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                Component[] components = prefab.GetComponentsInChildren<Component>(true);
                
                // If none of the components are null (if they are all not null) then continue
                if (components.All(component => component != null)) { continue; }

                Debug.LogError("Missing script found in prefab: " + assetPath, prefab);
                missingScriptCount++;
            }

            Debug.Log(missingScriptCount == 0
                ? "No missing scripts found in any prefabs."
                : $"{missingScriptCount} prefabs with missing scripts found.");
        }

        [MenuItem("Tools/Find Missing Scripts/In Scenes")]
        private static void FindMissingScriptsInScenes()
        {
            List< (string sceneName, string goScriptIsAttachedTo)> missingScripts = new();

            // Store the currently open scene, so that it can be opened when the operation is complete.
            var previouslyOpenScene = SceneManager.GetActiveScene().path;

            // Iterate over all the scenes
            foreach (string scenePath in EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes))
            {
                // Open the scene
                EditorSceneManager.OpenScene(scenePath);

                // Get all the game objects
                GameObject[] gameObjects = Object.FindObjectsOfType<GameObject>();

                // Iterate over all the game objects
                foreach (GameObject go in gameObjects)
                {
                    // Get all the components
                    Component[] components = go.GetComponents<Component>();
                    foreach (Component component in components)
                    {
                        // Skip if the component is there.
                        if (component != null) continue;
                        
                        // Record details of the missing script.
                        missingScripts.Add((go.GetFullPath(), Path.GetFileNameWithoutExtension(scenePath)));
                    }
                }
            }

            // Log the details of each missing script.
            foreach ((string sceneName, string goScriptIsAttachedTo) in missingScripts)
            {
                Debug.LogError($"Script is missing from GameObject \"{goScriptIsAttachedTo}\" in scene \"{sceneName}\".");
            }

            // Reopen the scene that was open originally
            EditorSceneManager.OpenScene(previouslyOpenScene);
        }

        #endregion

        [MenuItem("Tools/Find Unreferenced Prefabs",       isValidateFunction: true)]
        [MenuItem("Tools/Find Missing Scripts/In Scenes",  isValidateFunction: true)]
        [MenuItem("Tools/Find Missing Scripts/In Prefabs", isValidateFunction: true)]
        public static bool IsNotInPlayMode()
        {
            // TODO: The attribute "MenuItem" has a parameter for defining the EditorModes in which it should be enabled
            //       making use of that parameter would be a much cleaner solution.
            return !EditorApplication.isPlaying;
        }

        #region Methods to find unreferenced prefabs

        [MenuItem("Tools/Find Unreferenced Prefabs")]
        public static void FindAllUnreferencedPrefabs()
        {
            // Tell the user that it will take a little while
            if (false == EditorUtility.DisplayDialog("Find unreferenced prefabs",
                    "Finding unreferenced prefabs can take a while, and when the process is done all project scenes will be open in the editor.",
                    "Ok", "Cancel"))
            {
                return;
            }

            HashSet<string> referencedPrefabGuids = new();
            HashSet<string> checkedPrefabGuids = new();

            // First pass: Find prefabs referenced in scenes
            foreach (var scenePath in EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes))
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                
                GetPrefabGuidsInCurrentScene(ref referencedPrefabGuids);
            }

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
            Debug.Log($"There were a total of {allPrefabGuids.Length - referencedPrefabGuids.Count} unreferenced prefabs.");
            foreach (var prefabGuid in allPrefabGuids)
            {
                // If the prefab has been referenced, then continue
                if (referencedPrefabGuids.Contains(prefabGuid)) { continue; }

                string path = AssetDatabase.GUIDToAssetPath(prefabGuid);
                Debug.Log("Unreferenced Prefab: " + path);
            }
        }

        /// <summary>
        /// Adds the GUIDs of all prefabs in the current scene to the given HashSet of Guids
        /// </summary>
        /// <param name="guids">The HashSet to add the Guids to.</param>
        private static void GetPrefabGuidsInCurrentScene(ref HashSet<string> guids)
        {
            GameObject[] allGameObjects = Object.FindObjectsOfType<GameObject>(true);
            foreach (var go in allGameObjects)
            {
                var prefabGuidsOnGameObject = go.GetPrefabGuids();
                foreach (var guid in prefabGuidsOnGameObject)
                {
                    guids.Add(guid);
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

            var prefabsOnPrefabs = prefab.GetPrefabGuids();
            foreach (var prefabChild in prefabsOnPrefabs)
            {
                referencedPrefabGuids.Add(prefabChild);
            }
            
            // Check if this prefab references other prefabs and add them to the list
            Component[] components = prefab.GetComponentsInChildren<Component>(true);
            foreach (Component component in components)
            {
                // skip if the component is null
                if (null == component) continue;

                SerializedObject so = new(component);
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

                    // If the prefab has been referenced, then continue
                    if (referencedPrefabGuids.Contains(depGuid)) { continue; }

                    referencedPrefabGuids.Add(depGuid);
                    FindPrefabDependencies(ref checkedPrefabGuids, ref referencedPrefabGuids,
                        depGuid); // Recursively check this new prefab
                }
            }
        }

        #endregion
    }
}
#endif