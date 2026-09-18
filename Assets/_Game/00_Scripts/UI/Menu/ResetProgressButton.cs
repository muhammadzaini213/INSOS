using UnityEngine;
using Slafurry.System.Checkpoint;

namespace Slafurry.UI.Menu
{
    /// <summary>
    /// Reset progress button for settings menu.
    /// Provides methods to reset individual sections or all progress.
    /// 
    /// Integration:
    ///   1. Add this component to settings menu GameObject
    ///   2. Create buttons: "Reset Section 1", "Reset Section 2", "Reset Section 3", "Reset All"
    ///   3. Wire button OnClick events to methods below
    ///   4. (Recommended) Add confirmation dialog before resetting
    /// </summary>
    public class ResetProgressButton : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Show confirmation dialog before resetting (recommended)")]
        [SerializeField] private bool requireConfirmation = true;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // === PUBLIC METHODS (Wire to button OnClick events) ===
        
        /// <summary>
        /// Reset all progress (all sections).
        /// Wire to "Reset All Progress" button.
        /// </summary>
        public void OnResetAllProgressClicked()
        {
            if (CheckpointManager.Instance == null)
            {
                Debug.LogError("[ResetProgressButton] CheckpointManager not initialized!");
                return;
            }
            
            if (requireConfirmation)
            {
                // TODO: Show confirmation dialog
                // For now, log warning
                Debug.LogWarning("[ResetProgressButton] Reset All Progress requested. Implement confirmation dialog!");
                
                // Temporary: Require double-click or hold confirmation
                // You can integrate with a dialog system here
            }
            
            CheckpointManager.Instance.ClearAllCheckpoints();
            
            if (showDebugLogs)
                Debug.Log("[ResetProgressButton] ✓ All progress reset!");
        }
        
        /// <summary>
        /// Reset Section 1 progress.
        /// Wire to "Reset Section 1" button.
        /// </summary>
        public void OnResetSection1Clicked()
        {
            ResetSection(1);
        }
        
        /// <summary>
        /// Reset Section 2 progress.
        /// Wire to "Reset Section 2" button.
        /// </summary>
        public void OnResetSection2Clicked()
        {
            ResetSection(2);
        }
        
        /// <summary>
        /// Reset Section 3 progress.
        /// Wire to "Reset Section 3" button.
        /// </summary>
        public void OnResetSection3Clicked()
        {
            ResetSection(3);
        }
        
        // === PRIVATE HELPERS ===
        
        private void ResetSection(int section)
        {
            if (CheckpointManager.Instance == null)
            {
                Debug.LogError("[ResetProgressButton] CheckpointManager not initialized!");
                return;
            }
            
            if (requireConfirmation)
            {
                // TODO: Show confirmation dialog
                Debug.LogWarning($"[ResetProgressButton] Reset Section {section} requested. Implement confirmation dialog!");
            }
            
            CheckpointManager.Instance.ResetSectionProgress(section);
            
            if (showDebugLogs)
                Debug.Log($"[ResetProgressButton] ✓ Section {section} progress reset!");
        }
        
        // === DEBUG HELPERS (Editor only) ===
        
#if UNITY_EDITOR
        [ContextMenu("Debug: Reset All Progress (No Confirmation)")]
        private void DebugResetAll()
        {
            bool oldConfirmation = requireConfirmation;
            requireConfirmation = false;
            OnResetAllProgressClicked();
            requireConfirmation = oldConfirmation;
        }
        
        [ContextMenu("Debug: Print All Checkpoints")]
        private void DebugPrintCheckpoints()
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
#endif
    }
}
