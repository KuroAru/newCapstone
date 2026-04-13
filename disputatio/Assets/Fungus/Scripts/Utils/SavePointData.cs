// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

#if UNITY_5_3_OR_NEWER

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace Fungus
{
    /// <summary>
    /// Serializable container for a Save Point's data. 
    /// All data is stored as strings, and the only concrete game class it depends on is the SaveData component.
    /// </summary>
    [System.Serializable]
    public class SavePointData
    {
        [SerializeField] protected string savePointKey;

        [SerializeField] protected string savePointDescription;

        [SerializeField] protected string sceneName;

        [SerializeField] protected List<SaveDataItem> saveDataItems = new List<SaveDataItem>();

        protected static SavePointData Create(string _savePointKey, string _savePointDescription, string _sceneName)
        {
            var savePointData = new SavePointData();

            savePointData.savePointKey = _savePointKey;
            savePointData.savePointDescription = _savePointDescription;
            savePointData.sceneName = _sceneName;

            return savePointData;
        }

        #region Public methods

        /// <summary>
        /// Gets or sets the unique key for the Save Point.
        /// </summary>
        public string SavePointKey { get { return savePointKey; } set { savePointKey = value; } }

        /// <summary>
        /// Gets or sets the description for the Save Point.
        /// </summary>
        public string SavePointDescription { get { return savePointDescription; } set { savePointDescription = value; } }

        /// <summary>
        /// Gets or sets the scene name associated with the Save Point.
        /// </summary>
        public string SceneName { get { return sceneName; } set { sceneName = value; } }

        /// <summary>
        /// Gets the list of save data items.
        /// </summary>
        /// <value>The save data items.</value>
        public List<SaveDataItem> SaveDataItems { get { return saveDataItems; } }

        /// <summary>
        /// Encodes a new Save Point to data and converts it to JSON text format.
        /// </summary>
        public static string Encode(string _savePointKey, string _savePointDescription, string _sceneName)
        {
            var savePointData = Create(_savePointKey, _savePointDescription, _sceneName);

            // Look for a SaveData component in the scene to populate the save data items.
            var saveData = UnityEngine.Object.FindFirstObjectByType<SaveData>(FindObjectsInactive.Include);
            if (saveData != null)
            {
                saveData.Encode(savePointData.SaveDataItems);
            }

            return JsonUtility.ToJson(savePointData, true);
        }

        /// <summary>
        /// Decodes a Save Point from JSON text format and loads it.
        /// If the active scene already matches the save point scene, applies data without reloading (avoids
        /// double LoadScene and fixes loadAction/ExecuteBlocks ordering with SaveManager.Update).
        /// </summary>
        public static void Decode(string saveDataJSON)
        {
            var savePointData = JsonUtility.FromJson<SavePointData>(saveDataJSON);
            if (savePointData == null)
            {
                Debug.LogError("[Fungus SaveRestore] Decode failed: JSON did not deserialize to SavePointData.");
                return;
            }

            string activeScene = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(savePointData.SceneName) &&
                string.Equals(activeScene, savePointData.SceneName, StringComparison.Ordinal))
            {
                Debug.Log($"[Fungus SaveRestore] In-scene restore (no reload). scene={activeScene}, savePointKey={savePointData.savePointKey}");
                ApplyLoadedSavePointToScene(savePointData);
                return;
            }

            Debug.Log($"[Fungus SaveRestore] Loading scene for restore: saved={savePointData.SceneName}, activeNow={activeScene}, savePointKey={savePointData.savePointKey}");

            UnityAction<Scene, LoadSceneMode> onSceneLoadedAction = null;

            onSceneLoadedAction = (scene, mode) =>  {
                // Additive scene loads and non-matching scene loads could happen if the client is using the
                // SceneManager directly. We just ignore these events and hope they know what they're doing!
                if (mode == LoadSceneMode.Additive ||
                    scene.name != savePointData.SceneName)
                {
                    Debug.LogWarning(
                        $"[Fungus SaveRestore] sceneLoaded skipped restore: mode={mode}, scene.name={scene.name}, expected={savePointData.SceneName}");
                    return;
                }

                SceneManager.sceneLoaded -= onSceneLoadedAction;

                ApplyLoadedSavePointToScene(savePointData);
            };
                
            SceneManager.sceneLoaded += onSceneLoadedAction;
            SceneManager.LoadScene(savePointData.SceneName);
        }

        /// <summary>
        /// Applies decoded save items and notifies listeners (same as post-LoadScene path).
        /// </summary>
        static void ApplyLoadedSavePointToScene(SavePointData savePointData)
        {
            var saveData = UnityEngine.Object.FindFirstObjectByType<SaveData>(FindObjectsInactive.Include);
            int itemCount = savePointData.SaveDataItems != null ? savePointData.SaveDataItems.Count : 0;

            if (saveData == null)
            {
                Debug.LogWarning("[Fungus SaveRestore] No SaveData in scene; variables/narrative log will not restore.");
            }
            else if (itemCount == 0)
            {
                Debug.LogWarning("[Fungus SaveRestore] Save point has no SaveDataItems; check SaveData.flowcharts when saving.");
            }

            if (saveData != null && itemCount > 0)
            {
                saveData.Decode(savePointData.SaveDataItems);
            }

            SaveManagerSignals.DoSavePointLoaded(savePointData.savePointKey);
        }

        #endregion
    }
}

#endif