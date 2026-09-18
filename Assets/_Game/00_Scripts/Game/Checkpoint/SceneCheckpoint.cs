using UnityEngine;
using UnityEngine.SceneManagement;
using Slafurry.System.Checkpoint;

namespace Slafurry.Game.Checkpoint
{
    /// <summary>
    /// Auto-save checkpoint when scene loads. 
    /// Attach to a GameObject in each gameplay scene.
    /// Automatically detects section number from scene name.
    /// 
    /// Scene name pattern: "XX_Section Y" where Y is 1, 2, or 3
    /// Examples:
    ///   "01_Section 1" → Section 1
    ///   "05_Section 2" → Section 2
    ///   "10_Section 3" → Section 3
    ///   "StartMenu" → Skip (not a gameplay scene)
    /// </summary>
    public class SceneCheckpoint : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool autoSaveOnStart = true;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        void Start()
        {
            if (autoSaveOnStart)
            {
                SaveCheckpointForCurrentScene();
            }
        }
        
        /// <summary>
        /// Manually trigger checkpoint save (for custom checkpoint triggers).
        /// </summary>
        public void SaveCheckpoint()
        {
            SaveCheckpointForCurrentScene();
        }
        
        private void SaveCheckpointForCurrentScene()
        {
            // Wait for CheckpointManager to initialize
            if (CheckpointManager.Instance == null)
            {
                UnityEngine.Debug.LogWarning("[SceneCheckpoint] CheckpointManager not initialized yet. Retrying...");
                Invoke(nameof(SaveCheckpointForCurrentScene), 0.5f);
                return;
            }
            
            string sceneName = SceneManager.GetActiveScene().name;
            
            // Parse section number from scene name (e.g., "05_Section 1" → 1)
            int section = ParseSectionFromSceneName(sceneName);
            
            if (section == 0)
            {
            if (showDebugLogs)
                UnityEngine.Debug.Log($"[SceneCheckpoint] Scene '{sceneName}' is not a gameplay scene (no section detected). Skipping checkpoint.");
                return;
            }
            
            // Save checkpoint
            CheckpointManager.Instance.SaveCheckpoint(section, sceneName);
            
            if (showDebugLogs)
                UnityEngine.Debug.Log($"[SceneCheckpoint] ✓ Checkpoint saved: Section {section}, Scene '{sceneName}'");
        }
        
        /// <summary>
        /// Parse section number from scene name.
        /// Examples:
        ///   "01_Section 1" → 1
        ///   "05_Section 2" → 2
        ///   "10_Section 3" → 3
        ///   "StartMenu" → 0 (invalid)
        ///   "02_Section 1_1" → 1 (handles sub-variations)
        /// </summary>
        private int ParseSectionFromSceneName(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return 0;
            
            // Look for pattern "Section X" where X is 1, 2, or 3
            if (sceneName.Contains("Section 1"))
                return 1;
            if (sceneName.Contains("Section 2"))
                return 2;
            if (sceneName.Contains("Section 3"))
                return 3;
            
            return 0; // Not a gameplay scene
        }
        
        // === DEBUG HELPERS (Editor only) ===
        
#if UNITY_EDITOR
        [ContextMenu("Test: Save Checkpoint Now")]
        private void TestSaveCheckpoint()
        {
            SaveCheckpointForCurrentScene();
        }
        
        [ContextMenu("Test: Print Section Number")]
        private void TestPrintSection()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            int section = ParseSectionFromSceneName(sceneName);
            UnityEngine.Debug.Log($"Scene: '{sceneName}' → Section: {section}");
        }
#endif
    }
}
