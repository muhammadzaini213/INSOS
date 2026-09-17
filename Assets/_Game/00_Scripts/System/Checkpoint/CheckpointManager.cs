using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    /// </summary>
    public class CheckpointManager : GameSystem<CheckpointManager>
    {
        // === CONSTANTS ===
        
        private const string CHECKPOINT_PREFIX = "Checkpoint_Section";
        private const string LAST_SECTION_KEY = "LastPlayedSection";
        
        // Default start scenes for each section
        private readonly Dictionary<int, string> _defaultScenes = new()
        {
            { 1, "01_Section 1" },
            { 2, "01_Section 2" },
            { 3, "01_Section 3" }
        };
        
        // === EVENTS ===
        
        public event Action<int, string> OnCheckpointSaved;  // (section, sceneName)
        public event Action<int> OnCheckpointReset;          // (section)
        
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
            OnCheckpointSaved?.Invoke(section, sceneName);
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
                OnCheckpointReset?.Invoke(section);
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
        
        // === PRIVATE HELPERS ===
        
        private string GetCheckpointKey(int section)
        {
            return $"{CHECKPOINT_PREFIX}{section}";
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
                CheckpointData data = LoadCheckpointProgress(section);
                if (data != null)
                {
                    Debug.Log($"Section {section}: {data.sceneName} | Gender: {data.gender} | {data.GetTimestampText()}");
                }
                else
                {
                    Debug.Log($"Section {section}: No checkpoint");
                }
            }
            
            int lastSection = GetLastPlayedSection();
            Debug.Log($"Last Played Section: {(lastSection > 0 ? lastSection.ToString() : "None")}");
            Debug.Log("============================");
        }
    }
}
