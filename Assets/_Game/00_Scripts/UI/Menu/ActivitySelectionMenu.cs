using Slafurry.System.Checkpoint;
using Slafurry.System.Scene;
using UnityEngine;
using UnityEngine.UI;

namespace Slafurry.UI.Menu
{
    /// <summary>
    /// Activity Selection Menu - loads checkpoints or starts new section.
    /// Attach to 03_ChooseActivityMenu scene.
    ///
    /// When player clicks an activity button:
    ///   - If checkpoint exists → Load checkpoint scene (resume progress)
    ///   - If no checkpoint → Load default start scene (new game)
    ///
    /// Integration:
    ///   1. Attach this component to Canvas or UI root in 03_ChooseActivityMenu
    ///   2. Assign button references in Inspector
    ///   3. Wire button OnClick events to methods below
    ///   4. (Optional) Assign continue indicator GameObjects
    /// </summary>
    public class ActivitySelectionMenu : MonoBehaviour
    {
        [Header("Activity Buttons")]
        [Tooltip("Morning Activity button (Section 1)")]
        [SerializeField]
        private Button morningActivityButton;

        [Tooltip("Cleaning button (Section 2)")]
        [SerializeField]
        private Button cleaningButton;

        [Tooltip("Card Game button (Section 3)")]
        [SerializeField]
        private Button cardGameButton;

        [Header("Continue Indicators (Optional)")]
        [Tooltip("Show this icon/text when Section 1 has checkpoint (e.g., '▶ Continue')")]
        [SerializeField]
        private GameObject section1ContinueIcon;

        [Tooltip("Show this icon/text when Section 2 has checkpoint")]
        [SerializeField]
        private GameObject section2ContinueIcon;

        [Tooltip("Show this icon/text when Section 3 has checkpoint")]
        [SerializeField]
        private GameObject section3ContinueIcon;

        [Header("Debug")]
        [SerializeField]
        private bool showDebugLogs = true;

        void Start()
        {
            UpdateContinueIndicators();
        }

        void OnEnable()
        {
            // Update indicators when menu opens (in case checkpoints changed)
            UpdateContinueIndicators();
        }

        /// <summary>
        /// Update continue indicators based on checkpoint existence.
        /// Shows "Continue" icon if checkpoint exists, hides if not.
        /// </summary>
        private void UpdateContinueIndicators()
        {
            // Wait for CheckpointManager to initialize
            if (CheckpointManager.Instance == null)
            {
                if (showDebugLogs)
                    Debug.Log(
                        "[ActivitySelectionMenu] CheckpointManager not ready yet, skipping indicator update"
                    );
                return;
            }

            // Update Section 1 indicator
            if (section1ContinueIcon != null)
            {
                bool hasCheckpoint = CheckpointManager.Instance.HasCheckpoint(1);
                section1ContinueIcon.SetActive(hasCheckpoint);
                if (showDebugLogs)
                    Debug.Log(
                        $"[ActivitySelectionMenu] Section 1 continue indicator: {hasCheckpoint}"
                    );
            }

            // Update Section 2 indicator
            if (section2ContinueIcon != null)
            {
                bool hasCheckpoint = CheckpointManager.Instance.HasCheckpoint(2);
                section2ContinueIcon.SetActive(hasCheckpoint);
                if (showDebugLogs)
                    Debug.Log(
                        $"[ActivitySelectionMenu] Section 2 continue indicator: {hasCheckpoint}"
                    );
            }

            // Update Section 3 indicator
            if (section3ContinueIcon != null)
            {
                bool hasCheckpoint = CheckpointManager.Instance.HasCheckpoint(3);
                section3ContinueIcon.SetActive(hasCheckpoint);
                if (showDebugLogs)
                    Debug.Log(
                        $"[ActivitySelectionMenu] Section 3 continue indicator: {hasCheckpoint}"
                    );
            }
        }

        // === BUTTON CALLBACKS ===
        // Wire these methods to button OnClick events in Inspector

        /// <summary>
        /// Morning Activity button callback (Section 1).
        /// Loads checkpoint scene if exists, otherwise loads "01_Section 1".
        /// </summary>
        public void OnMorningActivityClicked()
        {
            if (CheckpointManager.Instance == null)
            {
                Debug.LogError("[ActivitySelectionMenu] CheckpointManager not initialized!");
                return;
            }

            string sceneToLoad = CheckpointManager.Instance.GetSceneToLoad(1);

            if (showDebugLogs)
                Debug.Log(
                    $"[ActivitySelectionMenu] Morning Activity clicked → Loading: {sceneToLoad}"
                );

            SceneSystem.Load(sceneToLoad);
        }

        /// <summary>
        /// Card Game button callback (Section 3).
        /// Loads checkpoint scene if exists, otherwise loads "01_Section 3".
        /// </summary>
        public void OnCardGameClicked()
        {
            if (CheckpointManager.Instance == null)
            {
                Debug.LogError("[ActivitySelectionMenu] CheckpointManager not initialized!");
                return;
            }

            string sceneToLoad = CheckpointManager.Instance.GetSceneToLoad(3);

            if (showDebugLogs)
                Debug.Log($"[ActivitySelectionMenu] Card Game clicked → Loading: {sceneToLoad}");

            SceneSystem.Load(sceneToLoad);
        }

        /// <summary>
        /// Cleaning button callback (Section 2).
        /// Loads checkpoint scene if exists, otherwise loads "01_Section 2".
        /// </summary>
        public void OnCleaningClicked()
        {
            if (CheckpointManager.Instance == null)
            {
                Debug.LogError("[ActivitySelectionMenu] CheckpointManager not initialized!");
                return;
            }

            string sceneToLoad = CheckpointManager.Instance.GetSceneToLoad(2);

            if (showDebugLogs)
                Debug.Log($"[ActivitySelectionMenu] Cleaning clicked → Loading: {sceneToLoad}");

            SceneSystem.Load(sceneToLoad);
        }

        // === DEBUG HELPERS (Editor only) ===

#if UNITY_EDITOR
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

        [ContextMenu("Debug: Clear All Checkpoints")]
        private void DebugClearCheckpoints()
        {
            if (CheckpointManager.Instance != null)
            {
                CheckpointManager.Instance.ClearAllCheckpoints();
                UpdateContinueIndicators();
                Debug.Log("All checkpoints cleared!");
            }
        }
#endif
    }
}
