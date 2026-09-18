using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Slafurry.Core.Abstract;
using Slafurry.System.Player;

namespace Slafurry.System.Checkpoint
{
    /// <summary>
    /// Checkpoint system manager (Singleton).
    /// Handles saving/loading player progress per section using PlayerPrefs.
    /// 
    /// Usage:
    ///   CheckpointManager.Instance.SaveCheckpoint(1, "05_Section 1");
    ///   CheckpointData data = CheckpointManager.Instance.LoadCheckpointProgress(1);
    ///   CheckpointManager.Instance.ResetSectionProgress(1);
    /// 
    /// UnityEvent Support:
    ///   All methods are UnityEvent-compatible and can be wired directly in Inspector.
    /// </summary>
    public class CheckpointManager : GameSystem<CheckpointManager>
    {
        // === CONSTANTS ===
        
        private const string CHECKPOINT_PREFIX = "Checkpoint_Section";
        private const string SAVE_DATA_PREFIX = "SectionSaveData_";
        private const string LAST_SECTION_KEY = "LastPlayedSection";
        
        // Default start scenes for each section
        private readonly Dictionary<int, string> _defaultScenes = new()
        {
            { 1, "01_Section 1" },
            { 2, "01_Section 2" },
            { 3, "01_Section 3" }
        };
        
        // In-memory cache for section save data
        private readonly Dictionary<int, SectionSaveData> _sectionDataCache = new();
        
        // === EVENTS (Dual Event Pattern) ===
        
        // C# Events
        public event Action<int, string> OnCheckpointSaved;  // (section, sceneName)
        public event Action<int> OnCheckpointReset;          // (section)
        public event Action<int> OnSectionDataSaved;         // (section) - fired when section save data is saved
        public event Action<int> OnSectionDataLoaded;        // (section) - fired when section save data is loaded
        
        // UnityEvents (Inspector-assignable)
        [Header("UnityEvents (Optional)")]
        [SerializeField] private UnityEvent<int, string> onCheckpointSavedUnityEvent;
        [SerializeField] private UnityEvent<int> onCheckpointResetUnityEvent;
        [SerializeField] private UnityEvent<int> onSectionDataSavedUnityEvent;
        [SerializeField] private UnityEvent<int> onSectionDataLoadedUnityEvent;
        
        // === INITIALIZATION ===
        
        public override int Priority => 5; // Load early (before most systems)
        
        public override IEnumerator Initialize()
        {
            Debug.Log("[CheckpointManager] Initializing checkpoint system...");
            yield return null;
        }
        
        public override void PostInitialize()
        {
            Debug.Log("[CheckpointManager] Checkpoint system ready!");
        }
        
        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake(); // DontDestroyOnLoad
        }
        
        // === PUBLIC API ===
        
        /// <summary>
        /// Save checkpoint for current scene. Auto-called by SceneCheckpoint component.
        /// Stores: section number, scene name, gender, timestamp.
        /// </summary>
        /// <param name="section">Section number (1, 2, or 3)</param>
        /// <param name="sceneName">Full scene name (e.g., "05_Section 1")</param>
        public void SaveCheckpoint(int section, string sceneName)
        {
            // Validate section number
            if (section < 1 || section > 3)
            {
                Debug.LogError($"[CheckpointManager] Invalid section: {section}. Must be 1, 2, or 3.");
                return;
            }
            
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[CheckpointManager] Scene name cannot be empty.");
                return;
            }
            
            // Create checkpoint data
            var data = new CheckpointData(
                section,
                sceneName,
                (int)PlayerData.CurrentGender
            );
            
            // Serialize to JSON
            string json = JsonUtility.ToJson(data);
            
            // Save to PlayerPrefs
            string key = GetCheckpointKey(section);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.SetInt(LAST_SECTION_KEY, section);
            PlayerPrefs.Save();
            
            Debug.Log($"[CheckpointManager] Checkpoint saved: Section {section}, Scene '{sceneName}', Gender {PlayerData.CurrentGender}");
            
            // Invoke both C# event and UnityEvent
            OnCheckpointSaved?.Invoke(section, sceneName);
            onCheckpointSavedUnityEvent?.Invoke(section, sceneName);
        }
        
        /// <summary>
        /// Load checkpoint data for a section.
        /// Returns null if no checkpoint exists.
        /// </summary>
        /// <param name="section">Section number (1, 2, or 3)</param>
        /// <returns>CheckpointData or null if no checkpoint exists</returns>
        public CheckpointData LoadCheckpointProgress(int section)
        {
            if (section < 1 || section > 3)
            {
                Debug.LogError($"[CheckpointManager] Invalid section: {section}");
                return null;
            }
            
            string key = GetCheckpointKey(section);
            
            if (!PlayerPrefs.HasKey(key))
            {
                Debug.Log($"[CheckpointManager] No checkpoint found for section {section}");
                return null;
            }
            
            string json = PlayerPrefs.GetString(key);
            
            try
            {
                CheckpointData data = JsonUtility.FromJson<CheckpointData>(json);
                Debug.Log($"[CheckpointManager] Checkpoint loaded: Section {section}, Scene '{data.sceneName}'");
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CheckpointManager] Failed to parse checkpoint for section {section}: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Delete checkpoint for a section (reset to beginning).
        /// </summary>
        /// <param name="section">Section number (1, 2, or 3)</param>
        public void ResetSectionProgress(int section)
        {
            if (section < 1 || section > 3)
            {
                Debug.LogError($"[CheckpointManager] Invalid section: {section}");
                return;
            }
            
            string key = GetCheckpointKey(section);
            
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
                Debug.Log($"[CheckpointManager] Section {section} progress reset");
                
                // Invoke both C# event and UnityEvent
                OnCheckpointReset?.Invoke(section);
                onCheckpointResetUnityEvent?.Invoke(section);
            }
            else
            {
                Debug.Log($"[CheckpointManager] No checkpoint to reset for section {section}");
            }
        }
        
        /// <summary>
        /// Check if section has a saved checkpoint.
        /// </summary>
        /// <param name="section">Section number (1, 2, or 3)</param>
        /// <returns>True if checkpoint exists</returns>
        public bool HasCheckpoint(int section)
        {
            if (section < 1 || section > 3)
                return false;
            
            string key = GetCheckpointKey(section);
            return PlayerPrefs.HasKey(key);
        }
        
        /// <summary>
        /// Get the last played section number (1, 2, or 3).
        /// Returns 0 if no checkpoints exist.
        /// </summary>
        /// <returns>Last played section or 0 if no checkpoints</returns>
        public int GetLastPlayedSection()
        {
            return PlayerPrefs.GetInt(LAST_SECTION_KEY, 0);
        }
        
        /// <summary>
        /// Clear all checkpoints (for "Reset All Progress" button).
        /// UnityEvent-compatible (no parameters).
        /// </summary>
        public void ClearAllCheckpoints()
        {
            for (int section = 1; section <= 3; section++)
            {
                string key = GetCheckpointKey(section);
                if (PlayerPrefs.HasKey(key))
                {
                    PlayerPrefs.DeleteKey(key);
                }
            }
            
            PlayerPrefs.DeleteKey(LAST_SECTION_KEY);
            PlayerPrefs.Save();
            
            Debug.Log("[CheckpointManager] All checkpoints cleared");
        }
        
        // === UNITY EVENT WRAPPERS (For Inspector OnClick events) ===
        
        /// <summary>
        /// Save checkpoint for Section 1. UnityEvent wrapper.
        /// Wire to button OnClick in Inspector.
        /// </summary>
        public void SaveCheckpointSection1()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            SaveCheckpoint(1, sceneName);
        }
        
        /// <summary>
        /// Save checkpoint for Section 2. UnityEvent wrapper.
        /// Wire to button OnClick in Inspector.
        /// </summary>
        public void SaveCheckpointSection2()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            SaveCheckpoint(2, sceneName);
        }
        
        /// <summary>
        /// Save checkpoint for Section 3. UnityEvent wrapper.
        /// Wire to button OnClick in Inspector.
        /// </summary>
        public void SaveCheckpointSection3()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            SaveCheckpoint(3, sceneName);
        }
        
        /// <summary>
        /// Reset Section 1 progress. UnityEvent wrapper.
        /// Wire to button OnClick in Inspector.
        /// </summary>
        public void ResetSection1()
        {
            ResetSectionProgress(1);
        }
        
        /// <summary>
        /// Reset Section 2 progress. UnityEvent wrapper.
        /// Wire to button OnClick in Inspector.
        /// </summary>
        public void ResetSection2()
        {
            ResetSectionProgress(2);
        }
        
        /// <summary>
        /// Reset Section 3 progress. UnityEvent wrapper.
        /// Wire to button OnClick in Inspector.
        /// </summary>
        public void ResetSection3()
        {
            ResetSectionProgress(3);
        }
        
        /// <summary>
        /// Load Section 1 scene (checkpoint or default). UnityEvent wrapper.
        /// Wire to button OnClick in Inspector.
        /// </summary>
        public void LoadSection1Scene()
        {
            string scene = GetSceneToLoad(1);
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
        }
        
        /// <summary>
        /// Load Section 2 scene (checkpoint or default). UnityEvent wrapper.
        /// Wire to button OnClick in Inspector.
        /// </summary>
        public void LoadSection2Scene()
        {
            string scene = GetSceneToLoad(2);
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
        }
        
        /// <summary>
        /// Load Section 3 scene (checkpoint or default). UnityEvent wrapper.
        /// Wire to button OnClick in Inspector.
        /// </summary>
        public void LoadSection3Scene()
        {
            string scene = GetSceneToLoad(3);
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
        }
        
        /// <summary>
        /// Get scene name to load for a section (checkpoint scene or default start scene).
        /// Automatically restores gender from checkpoint if it exists.
        /// </summary>
        /// <param name="section">Section number (1, 2, or 3)</param>
        /// <returns>Scene name to load</returns>
        public string GetSceneToLoad(int section)
        {
            CheckpointData checkpoint = LoadCheckpointProgress(section);
            
            if (checkpoint != null)
            {
                // Restore gender from checkpoint
                PlayerData.SetGender((Gender)checkpoint.gender);
                Debug.Log($"[CheckpointManager] Loading checkpoint scene: {checkpoint.sceneName} (Gender restored: {PlayerData.CurrentGender})");
                return checkpoint.sceneName;
            }
            
            // No checkpoint, return default start scene
            string defaultScene = _defaultScenes.ContainsKey(section)
                ? _defaultScenes[section]
                : $"01_Section {section}";
            
            Debug.Log($"[CheckpointManager] No checkpoint found, loading default scene: {defaultScene}");
            return defaultScene;
        }
        
        // === SECTION SAVE DATA API ===
        
        /// <summary>
        /// Get section save data. Creates new data if none exists.
        /// Data is cached in memory for fast access.
        /// </summary>
        /// <param name="section">Section number (1, 2, or 3)</param>
        /// <returns>SectionSaveData for the section</returns>
        public SectionSaveData GetSectionData(int section)
        {
            if (section < 1 || section > 3)
            {
                Debug.LogError($"[CheckpointManager] Invalid section: {section}");
                return null;
            }
            
            // Return cached data if available
            if (_sectionDataCache.ContainsKey(section))
            {
                return _sectionDataCache[section];
            }
            
            // Try to load from PlayerPrefs
            string key = GetSaveDataKey(section);
            if (PlayerPrefs.HasKey(key))
            {
                string json = PlayerPrefs.GetString(key);
                try
                {
                    SectionSaveData data = JsonUtility.FromJson<SectionSaveData>(json);
                    _sectionDataCache[section] = data;
                    Debug.Log($"[CheckpointManager] Section {section} save data loaded from PlayerPrefs");
                    OnSectionDataLoaded?.Invoke(section);
                    onSectionDataLoadedUnityEvent?.Invoke(section);
                    return data;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[CheckpointManager] Failed to parse section {section} save data: {e.Message}");
                }
            }
            
            // Create new save data
            SectionSaveData newData = new SectionSaveData(section);
            _sectionDataCache[section] = newData;
            Debug.Log($"[CheckpointManager] Created new save data for section {section}");
            return newData;
        }
        
        /// <summary>
        /// Save section data to PlayerPrefs.
        /// Call this after modifying section data.
        /// </summary>
        /// <param name="section">Section number (1, 2, or 3)</param>
        public void SaveSectionData(int section)
        {
            if (section < 1 || section > 3)
            {
                Debug.LogError($"[CheckpointManager] Invalid section: {section}");
                return;
            }
            
            if (!_sectionDataCache.ContainsKey(section))
            {
                Debug.LogWarning($"[CheckpointManager] No data to save for section {section}");
                return;
            }
            
            SectionSaveData data = _sectionDataCache[section];
            data.lastPlayedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            string json = JsonUtility.ToJson(data);
            string key = GetSaveDataKey(section);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
            
            Debug.Log($"[CheckpointManager] Section {section} save data saved to PlayerPrefs");
            OnSectionDataSaved?.Invoke(section);
            onSectionDataSavedUnityEvent?.Invoke(section);
        }
        
        /// <summary>
        /// Reset section save data (clears all progress, items, tasks, etc.).
        /// Also clears the checkpoint for this section.
        /// </summary>
        /// <param name="section">Section number (1, 2, or 3)</param>
        public void ResetSectionData(int section)
        {
            if (section < 1 || section > 3)
            {
                Debug.LogError($"[CheckpointManager] Invalid section: {section}");
                return;
            }
            
            // Remove from cache
            if (_sectionDataCache.ContainsKey(section))
            {
                _sectionDataCache.Remove(section);
            }
            
            // Remove from PlayerPrefs
            string key = GetSaveDataKey(section);
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
            }
            
            // Also reset checkpoint
            ResetSectionProgress(section);
            
            Debug.Log($"[CheckpointManager] Section {section} save data reset");
        }
        
        /// <summary>
        /// Check if section has save data.
        /// </summary>
        public bool HasSectionData(int section)
        {
            if (section < 1 || section > 3)
                return false;
            
            return PlayerPrefs.HasKey(GetSaveDataKey(section));
        }
        
        // === PRIVATE HELPERS ===
        
        private string GetCheckpointKey(int section)
        {
            return $"{CHECKPOINT_PREFIX}{section}";
        }
        
        private string GetSaveDataKey(int section)
        {
            return $"{SAVE_DATA_PREFIX}{section}";
        }
        
        // === DEBUG HELPERS ===
        
        /// <summary>
        /// Get all checkpoint info for debugging (Editor only).
        /// </summary>
        public void DebugPrintAllCheckpoints()
        {
            Debug.Log("=== CHECKPOINT DEBUG INFO ===");
            
            for (int section = 1; section <= 3; section++)
            {
                // Checkpoint info
                CheckpointData data = LoadCheckpointProgress(section);
                if (data != null)
                {
                    Debug.Log($"Section {section} Checkpoint: {data.sceneName} | Gender: {data.gender} | {data.GetTimestampText()}");
                }
                else
                {
                    Debug.Log($"Section {section} Checkpoint: No checkpoint");
                }
                
                // Save data info
                if (HasSectionData(section))
                {
                    SectionSaveData saveData = GetSectionData(section);
                    Debug.Log($"Section {section} Save Data: {saveData.completedScenes.Count} scenes, {saveData.collectedItems.Count} items, {saveData.completedTasks.Count} tasks, {saveData.progressPercentage:F1}% complete");
                }
                else
                {
                    Debug.Log($"Section {section} Save Data: No save data");
                }
            }
            
            int lastSection = GetLastPlayedSection();
            Debug.Log($"Last Played Section: {(lastSection > 0 ? lastSection.ToString() : "None")}");
            Debug.Log("============================");
        }
    }
}
