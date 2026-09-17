using UnityEngine;
using Slafurry.System.Checkpoint;

namespace Slafurry.Game
{
    /// <summary>
    /// Helper component for managing section save data.
    /// Attach to gameplay objects to interact with section-specific save data.
    /// 
    /// Usage Examples:
    ///   - Mark scene as completed when player finishes
    ///   - Track collected items
    ///   - Store task progress
    ///   - Set completion flags
    /// </summary>
    public class SectionSaveDataHelper : MonoBehaviour
    {
        [Header("Section Settings")]
        [Tooltip("Which section this helper manages (1, 2, or 3)")]
        [SerializeField] private int section = 1;
        
        [Header("Auto-Save Settings")]
        [Tooltip("Auto-save when this script is destroyed (scene unload)")]
        [SerializeField] private bool autoSaveOnDestroy = true;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        private SectionSaveData _saveData;
        
        void Start()
        {
            LoadSectionData();
        }
        
        void OnDestroy()
        {
            if (autoSaveOnDestroy && _saveData != null)
            {
                SaveSectionData();
            }
        }
        
        // === PUBLIC API ===
        
        /// <summary>
        /// Load section save data from CheckpointManager.
        /// </summary>
        public void LoadSectionData()
        {
            if (CheckpointManager.Instance == null)
            {
                UnityEngine.UnityEngine.UnityEngine.Debug.LogError("[SectionSaveDataHelper] CheckpointManager not initialized!");
                return;
            }
            
            _saveData = CheckpointManager.Instance.GetSectionData(section);
            
            if (showDebugLogs)
                UnityEngine.UnityEngine.Debug.Log($"[SectionSaveDataHelper] Loaded save data for Section {section}");
        }
        
        /// <summary>
        /// Save section data to PlayerPrefs.
        /// </summary>
        public void SaveSectionData()
        {
            if (CheckpointManager.Instance == null)
            {
                UnityEngine.UnityEngine.Debug.LogError("[SectionSaveDataHelper] CheckpointManager not initialized!");
                return;
            }
            
            if (_saveData == null)
            {
                UnityEngine.UnityEngine.Debug.LogWarning("[SectionSaveDataHelper] No data to save!");
                return;
            }
            
            CheckpointManager.Instance.SaveSectionData(section);
            
            if (showDebugLogs)
                UnityEngine.Debug.Log($"[SectionSaveDataHelper] Saved data for Section {section}");
        }
        
        // === SCENE MANAGEMENT ===
        
        /// <summary>
        /// Mark current scene as completed.
        /// </summary>
        public void MarkCurrentSceneCompleted()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            MarkSceneCompleted(sceneName);
        }
        
        /// <summary>
        /// Mark a specific scene as completed.
        /// </summary>
        public void MarkSceneCompleted(string sceneName)
        {
            if (_saveData == null)
            {
                UnityEngine.UnityEngine.Debug.LogWarning("[SectionSaveDataHelper] Save data not loaded!");
                return;
            }
            
            if (!_saveData.completedScenes.Contains(sceneName))
            {
                _saveData.completedScenes.Add(sceneName);
                
                if (showDebugLogs)
                    UnityEngine.Debug.Log($"[SectionSaveDataHelper] Scene '{sceneName}' marked as completed");
            }
        }
        
        /// <summary>
        /// Check if a scene has been completed.
        /// </summary>
        public bool IsSceneCompleted(string sceneName)
        {
            if (_saveData == null)
                return false;
            
            return _saveData.completedScenes.Contains(sceneName);
        }
        
        // === ITEM MANAGEMENT ===
        
        /// <summary>
        /// Add collected item to save data.
        /// </summary>
        public void CollectItem(string itemId)
        {
            if (_saveData == null)
            {
                UnityEngine.UnityEngine.Debug.LogWarning("[SectionSaveDataHelper] Save data not loaded!");
                return;
            }
            
            if (!_saveData.collectedItems.Contains(itemId))
            {
                _saveData.collectedItems.Add(itemId);
                
                if (showDebugLogs)
                    UnityEngine.Debug.Log($"[SectionSaveDataHelper] Item '{itemId}' collected");
            }
        }
        
        /// <summary>
        /// Check if item has been collected.
        /// </summary>
        public bool HasItem(string itemId)
        {
            if (_saveData == null)
                return false;
            
            return _saveData.collectedItems.Contains(itemId);
        }
        
        // === TASK MANAGEMENT ===
        
        /// <summary>
        /// Mark a task as completed.
        /// </summary>
        public void CompleteTask(string taskId)
        {
            if (_saveData == null)
            {
                UnityEngine.UnityEngine.Debug.LogWarning("[SectionSaveDataHelper] Save data not loaded!");
                return;
            }
            
            if (!_saveData.completedTasks.Contains(taskId))
            {
                _saveData.completedTasks.Add(taskId);
                
                if (showDebugLogs)
                    UnityEngine.Debug.Log($"[SectionSaveDataHelper] Task '{taskId}' completed");
            }
        }
        
        /// <summary>
        /// Check if task is completed.
        /// </summary>
        public bool IsTaskCompleted(string taskId)
        {
            if (_saveData == null)
                return false;
            
            return _saveData.completedTasks.Contains(taskId);
        }
        
        /// <summary>
        /// Increment task progress counter.
        /// </summary>
        public void IncrementTaskProgress(string taskId, int amount = 1)
        {
            if (_saveData == null)
            {
                UnityEngine.UnityEngine.Debug.LogWarning("[SectionSaveDataHelper] Save data not loaded!");
                return;
            }
            
            if (!_saveData.taskProgress.ContainsKey(taskId))
            {
                _saveData.taskProgress[taskId] = 0;
            }
            
            _saveData.taskProgress[taskId] += amount;
            
            if (showDebugLogs)
                UnityEngine.Debug.Log($"[SectionSaveDataHelper] Task '{taskId}' progress: {_saveData.taskProgress[taskId]}");
        }
        
        /// <summary>
        /// Get task progress counter value.
        /// </summary>
        public int GetTaskProgress(string taskId)
        {
            if (_saveData == null || !_saveData.taskProgress.ContainsKey(taskId))
                return 0;
            
            return _saveData.taskProgress[taskId];
        }
        
        // === FLAG MANAGEMENT ===
        
        /// <summary>
        /// Set a boolean flag.
        /// </summary>
        public void SetFlag(string flagId, bool value)
        {
            if (_saveData == null)
            {
                UnityEngine.UnityEngine.Debug.LogWarning("[SectionSaveDataHelper] Save data not loaded!");
                return;
            }
            
            _saveData.flags[flagId] = value;
            
            if (showDebugLogs)
                UnityEngine.Debug.Log($"[SectionSaveDataHelper] Flag '{flagId}' = {value}");
        }
        
        /// <summary>
        /// Get a boolean flag value.
        /// </summary>
        public bool GetFlag(string flagId, bool defaultValue = false)
        {
            if (_saveData == null || !_saveData.flags.ContainsKey(flagId))
                return defaultValue;
            
            return _saveData.flags[flagId];
        }
        
        // === CUSTOM DATA ===
        
        /// <summary>
        /// Store custom string data.
        /// </summary>
        public void SetCustomData(string key, string value)
        {
            if (_saveData == null)
            {
                UnityEngine.UnityEngine.Debug.LogWarning("[SectionSaveDataHelper] Save data not loaded!");
                return;
            }
            
            _saveData.customData[key] = value;
        }
        
        /// <summary>
        /// Get custom string data.
        /// </summary>
        public string GetCustomData(string key, string defaultValue = "")
        {
            if (_saveData == null || !_saveData.customData.ContainsKey(key))
                return defaultValue;
            
            return _saveData.customData[key];
        }
        
        // === STATISTICS ===
        
        /// <summary>
        /// Increment death/failure counter.
        /// </summary>
        public void IncrementDeathCount()
        {
            if (_saveData == null)
                return;
            
            _saveData.deathCount++;
            
            if (showDebugLogs)
                UnityEngine.Debug.Log($"[SectionSaveDataHelper] Death count: {_saveData.deathCount}");
        }
        
        /// <summary>
        /// Increment hints used counter.
        /// </summary>
        public void IncrementHintsUsed()
        {
            if (_saveData == null)
                return;
            
            _saveData.hintsUsed++;
            
            if (showDebugLogs)
                UnityEngine.Debug.Log($"[SectionSaveDataHelper] Hints used: {_saveData.hintsUsed}");
        }
        
        /// <summary>
        /// Set section completion status.
        /// </summary>
        public void SetSectionCompleted(bool completed)
        {
            if (_saveData == null)
                return;
            
            _saveData.isCompleted = completed;
            
            if (showDebugLogs)
                UnityEngine.Debug.Log($"[SectionSaveDataHelper] Section {section} completed: {completed}");
        }
        
        /// <summary>
        /// Set progress percentage (0-100).
        /// </summary>
        public void SetProgress(float percentage)
        {
            if (_saveData == null)
                return;
            
            _saveData.progressPercentage = Mathf.Clamp(percentage, 0f, 100f);
            
            if (showDebugLogs)
                UnityEngine.Debug.Log($"[SectionSaveDataHelper] Progress: {_saveData.progressPercentage:F1}%");
        }
        
        // === GETTERS ===
        
        public SectionSaveData GetSaveData() => _saveData;
        public int GetCompletedSceneCount() => _saveData?.completedScenes.Count ?? 0;
        public int GetCollectedItemCount() => _saveData?.collectedItems.Count ?? 0;
        public int GetCompletedTaskCount() => _saveData?.completedTasks.Count ?? 0;
        public float GetProgress() => _saveData?.progressPercentage ?? 0f;
        public bool IsSectionCompleted() => _saveData?.isCompleted ?? false;
    }
}
