using UnityEngine;
using UnityEngine.Events;
using Slafurry.System.Checkpoint;

namespace Slafurry.UI.Menu
{
    /// <summary>
    /// UnityEvent-compatible checkpoint actions for UI buttons.
    /// Attach this component to UI elements or use as a central button handler.
    /// 
    /// Usage:
    ///   1. Add this component to Canvas or button GameObject
    ///   2. Wire button OnClick events to public methods in Inspector
    ///   3. All methods are parameterless for easy UnityEvent integration
    /// 
    /// Example:
    ///   Button OnClick() -> CheckpointButtons.LoadSection1()
    /// </summary>
    public class CheckpointButtons : MonoBehaviour
    {
        [Header("Optional: UnityEvents")]
        [Tooltip("Fires when any checkpoint is loaded")]
        [SerializeField] private UnityEvent onCheckpointLoaded;
        
        [Tooltip("Fires when any checkpoint is saved")]
        [SerializeField] private UnityEvent onCheckpointSaved;
        
        [Tooltip("Fires when any checkpoint is reset")]
        [SerializeField] private UnityEvent onCheckpointReset;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // === LOAD SECTION SCENES (with checkpoint resume) ===
        
        /// <summary>
        /// Load Section 1 scene (checkpoint or default start scene).
        /// Wire to button OnClick in Inspector.
        /// </summary>
        public void LoadSection1()
        {
            if (CheckpointManager.Instance == null)
            {
                Debug.LogError("[CheckpointButtons] CheckpointManager not initialized!");
                return;
            }
            
            string scene = CheckpointManager.Instance.GetSceneToLoad(1);
            if (showDebugLogs)
                Debug.Log($"[CheckpointButtons] Loading Section 1: {scene}");
            
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
            onCheckpointLoaded?.Invoke();
        }
        
        /// <summary>
        /// Load Section 2 scene (checkpoint or default start scene).
        /// Wire to button OnClick in Inspector.
        /// </summary>
        public void LoadSection2()
        {
            if (CheckpointManager.Instance == null)
            {
                Debug.LogError("[CheckpointButtons] CheckpointManager not initialized!");
                return;
            }
            
            string scene = CheckpointManager.Instance.GetSceneToLoad(2);
            if (showDebugLogs)
                Debug.Log($"[CheckpointButtons] Loading Section 2: {scene}");
            
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
            onCheckpointLoaded?.Invoke();
        }
        
        /// <summary>
        /// Load Section 3 scene (checkpoint or default start scene).
        /// Wire to button OnClick in Inspector.
        /// </summary>
        public void LoadSection3()
        {
            if (CheckpointManager.Instance == null)
            {
                Debug.LogError("[CheckpointButtons] CheckpointManager not initialized!");
                return;
            }
            
            string scene = CheckpointManager.Instance.GetSceneToLoad(3);
            if (showDebugLogs)
                Debug.Log($"[CheckpointButtons] Loading Section 3: {scene}");
            
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
            onCheckpointLoaded?.Invoke();
        }
        
        /// <summary>
        /// Load last played checkpoint (any section).
        /// Wire to "Continue" button OnClick in Inspector.
        /// </summary>
        public void LoadLastCheckpoint()
        {
            if (CheckpointManager.Instance == null)
            {
                Debug.LogError("[CheckpointButtons] CheckpointManager not initialized!");
                return;
            }
            
            int lastSection = CheckpointManager.Instance.GetLastPlayedSection();
            
            if (lastSection == 0)
            {
                Debug.LogWarning("[CheckpointButtons] No checkpoint found!");
                return;
            }
            
            string scene = CheckpointManager.Instance.GetSceneToLoad(lastSection);
            if (showDebugLogs)
                Debug.Log($"[CheckpointButtons] Loading last checkpoint (Section {lastSection}): {scene}");
            
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
            onCheckpointLoaded?.Invoke();
        }
        
        // === SAVE CHECKPOINTS ===
        
        /// <summary>
        /// Save checkpoint for Section 1 (current scene).
        /// Wire to button OnClick in Inspector.
        /// </summary>
        public void SaveCheckpointSection1()
        {
            if (CheckpointManager.Instance == null)
            {
                Debug.LogError("[CheckpointButtons] CheckpointManager not initialized!");
                return;
            }
            
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            CheckpointManager.Instance.SaveCheckpoint(1, sceneName);
            onCheckpointSaved?.Invoke();
        }
        
        /// <summary>
        /// Save checkpoint for Section 2 (current scene).
        /// Wire to button OnClick in Inspector.
        /// </summary>
        public void SaveCheckpointSection2()
        {
            if (CheckpointManager.Instance == null)
            {
                Debug.LogError("[CheckpointButtons] CheckpointManager not initialized!");
                return;
            }
            
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            CheckpointManager.Instance.SaveCheckpoint(2, sceneName);
            onCheckpointSaved?.Invoke();
        }
        
        /// <summary>
        /// Save checkpoint for Section 3 (current scene).
        /// Wire to button OnClick in Inspector.
        /// </summary>
        public void SaveCheckpointSection3()
        {
            if (CheckpointManager.Instance == null)
            {
                Debug.LogError("[CheckpointButtons] CheckpointManager not initialized!");
                return;
            }
            
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            CheckpointManager.Instance.SaveCheckpoint(3, sceneName);
            onCheckpointSaved?.Invoke();
        }
        
        // === RESET CHECKPOINTS ===
        
        /// <summary>
        /// Reset Section 1 progress.
        /// Wire to button OnClick in Inspector.
        /// </summary>
        public void ResetSection1()
        {
            if (CheckpointManager.Instance == null)
            {
                Debug.LogError("[CheckpointButtons] CheckpointManager not initialized!");
                return;
            }
            
            CheckpointManager.Instance.ResetSectionProgress(1);
            onCheckpointReset?.Invoke();
        }
        
        /// <summary>
        /// Reset Section 2 progress.
        /// Wire to button OnClick in Inspector.
        /// </summary>
        public void ResetSection2()
        {
            if (CheckpointManager.Instance == null)
            {
                Debug.LogError("[CheckpointButtons] CheckpointManager not initialized!");
                return;
            }
            
            CheckpointManager.Instance.ResetSectionProgress(2);
            onCheckpointReset?.Invoke();
        }
        
        /// <summary>
        /// Reset Section 3 progress.
        /// Wire to button OnClick in Inspector.
        /// </summary>
        public void ResetSection3()
        {
            if (CheckpointManager.Instance == null)
            {
                Debug.LogError("[CheckpointButtons] CheckpointManager not initialized!");
                return;
            }
            
            CheckpointManager.Instance.ResetSectionProgress(3);
            onCheckpointReset?.Invoke();
        }
        
        /// <summary>
        /// Reset all progress (all sections).
        /// Wire to button OnClick in Inspector.
        /// </summary>
        public void ResetAllProgress()
        {
            if (CheckpointManager.Instance == null)
            {
                Debug.LogError("[CheckpointButtons] CheckpointManager not initialized!");
                return;
            }
            
            CheckpointManager.Instance.ClearAllCheckpoints();
            onCheckpointReset?.Invoke();
        }
        
        // === DEBUG / UTILITY ===
        
        /// <summary>
        /// Print all checkpoint info to console.
        /// Wire to button OnClick in Inspector.
        /// </summary>
        public void PrintAllCheckpoints()
        {
            if (CheckpointManager.Instance != null)
            {
                CheckpointManager.Instance.DebugPrintAllCheckpoints();
            }
        }
    }
}
