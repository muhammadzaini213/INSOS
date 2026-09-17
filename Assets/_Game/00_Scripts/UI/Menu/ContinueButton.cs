using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Slafurry.System.Scene;
using Slafurry.System.Checkpoint;

namespace Slafurry.UI.Menu
{
    /// <summary>
    /// Continue button in main menu - loads last played checkpoint.
    /// Auto-disables if no checkpoints exist.
    /// 
    /// Integration:
    ///   1. Add this component to your "Continue" button GameObject
    ///   2. Assign button reference in Inspector
    ///   3. (Optional) Assign text component to show last played info
    ///   4. Wire OnContinueClicked() to button's OnClick event
    /// </summary>
    public class ContinueButton : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The Continue button component")]
        [SerializeField] private Button continueButton;
        
        [Tooltip("Optional: Text to show last played info (e.g., 'Continue: Section 1')")]
        [SerializeField] private TextMeshProUGUI infoText;
        
        [Header("Settings")]
        [Tooltip("Show section name in button text")]
        [SerializeField] private bool showSectionInfo = true;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        void Start()
        {
            UpdateButtonState();
        }
        
        void OnEnable()
        {
            UpdateButtonState();
        }
        
        /// <summary>
        /// Update button state based on checkpoint availability.
        /// Disables button if no checkpoints exist.
        /// </summary>
        private void UpdateButtonState()
        {
            // Wait for CheckpointManager to initialize
            if (CheckpointManager.Instance == null)
            {
                if (showDebugLogs)
                    Debug.Log("[ContinueButton] CheckpointManager not ready yet");
                
                if (continueButton != null)
                    continueButton.interactable = false;
                
                return;
            }
            
            int lastSection = CheckpointManager.Instance.GetLastPlayedSection();
            bool hasCheckpoint = lastSection > 0;
            
            // Update button interactability
            if (continueButton != null)
            {
                continueButton.interactable = hasCheckpoint;
            }
            
            // Update info text
            if (infoText != null && showSectionInfo)
            {
                if (hasCheckpoint)
                {
                    CheckpointData checkpoint = CheckpointManager.Instance.LoadCheckpointProgress(lastSection);
                    if (checkpoint != null)
                    {
                        // Show section info (e.g., "Continue: Section 1")
                        infoText.text = $"Continue: Section {lastSection}";
                        
                        // Or show last played time (e.g., "Last played: 2 hours ago")
                        // infoText.text = checkpoint.GetTimestampText();
                    }
                }
                else
                {
                    infoText.text = "No saved progress";
                }
            }
            
            if (showDebugLogs)
                Debug.Log($"[ContinueButton] Button state updated: {(hasCheckpoint ? "Enabled" : "Disabled")} (Last section: {lastSection})");
        }
        
        /// <summary>
        /// Button callback - load last played checkpoint.
        /// Wire this method to the button's OnClick event.
        /// </summary>
        public void OnContinueClicked()
        {
            if (CheckpointManager.Instance == null)
            {
                Debug.LogError("[ContinueButton] CheckpointManager not initialized!");
                return;
            }
            
            int lastSection = CheckpointManager.Instance.GetLastPlayedSection();
            
            if (lastSection == 0)
            {
                Debug.LogWarning("[ContinueButton] No checkpoint found! Button should be disabled.");
                return;
            }
            
            string sceneToLoad = CheckpointManager.Instance.GetSceneToLoad(lastSection);
            
            if (showDebugLogs)
                Debug.Log($"[ContinueButton] Continue clicked → Loading: {sceneToLoad}");
            
            SceneSystem.Load(sceneToLoad);
        }
        
        // === DEBUG HELPERS (Editor only) ===
        
#if UNITY_EDITOR
        [ContextMenu("Debug: Print Checkpoint Info")]
        private void DebugPrintInfo()
        {
            if (CheckpointManager.Instance != null)
            {
                CheckpointManager.Instance.DebugPrintAllCheckpoints();
            }
            else
            {
                Debug.LogWarning("CheckpointManager not initialized!");
            }
        }
        
        [ContextMenu("Test: Simulate Continue Click")]
        private void TestContinueClick()
        {
            OnContinueClicked();
        }
#endif
    }
}
