using UnityEngine;
using Slafurry.System.Checkpoint;
using Slafurry.System.Scene;

namespace Slafurry.Game.Debug
{
    /// <summary>
    /// Debug panel for testing checkpoint system in Editor.
    /// Attach to a Canvas in any scene for quick testing.
    /// 
    /// Usage:
    ///   1. Create Canvas in scene
    ///   2. Add this component to Canvas
    ///   3. Press Play
    ///   4. Use UI buttons or keyboard shortcuts to test
    /// </summary>
    public class CheckpointDebugPanel : MonoBehaviour
    {
        [Header("Keyboard Shortcuts")]
        [SerializeField] private bool enableKeyboardShortcuts = true;
        
        void Update()
        {
            if (!enableKeyboardShortcuts) return;
            
            // Save checkpoints
            if (Input.GetKeyDown(KeyCode.Alpha1))
                TestSaveSection1();
            if (Input.GetKeyDown(KeyCode.Alpha2))
                TestSaveSection2();
            if (Input.GetKeyDown(KeyCode.Alpha3))
                TestSaveSection3();
            
            // Load checkpoints
            if (Input.GetKeyDown(KeyCode.Q))
                TestLoadSection1();
            if (Input.GetKeyDown(KeyCode.W))
                TestLoadSection2();
            if (Input.GetKeyDown(KeyCode.E))
                TestLoadSection3();
            
            // Reset
            if (Input.GetKeyDown(KeyCode.R))
                TestResetAll();
            
            // Print debug info
            if (Input.GetKeyDown(KeyCode.P))
                TestPrintAll();
        }
        
        // === SAVE TESTS ===
        
        public void TestSaveSection1()
        {
            CheckpointManager.Instance.SaveCheckpoint(1, "05_Section 1");
            Debug.Log("✓ Saved checkpoint: Section 1, Scene '05_Section 1'");
        }
        
        public void TestSaveSection2()
        {
            CheckpointManager.Instance.SaveCheckpoint(2, "02_Section 2");
            Debug.Log("✓ Saved checkpoint: Section 2, Scene '02_Section 2'");
        }
        
        public void TestSaveSection3()
        {
            CheckpointManager.Instance.SaveCheckpoint(3, "03_Section 3");
            Debug.Log("✓ Saved checkpoint: Section 3, Scene '03_Section 3'");
        }
        
        // === LOAD TESTS ===
        
        public void TestLoadSection1()
        {
            CheckpointData data = CheckpointManager.Instance.LoadCheckpointProgress(1);
            if (data != null)
                Debug.Log($"✓ Section 1 checkpoint: {data.sceneName} | Gender: {data.gender} | {data.GetTimestampText()}");
            else
                Debug.Log("❌ No checkpoint for Section 1");
        }
        
        public void TestLoadSection2()
        {
            CheckpointData data = CheckpointManager.Instance.LoadCheckpointProgress(2);
            if (data != null)
                Debug.Log($"✓ Section 2 checkpoint: {data.sceneName} | Gender: {data.gender} | {data.GetTimestampText()}");
            else
                Debug.Log("❌ No checkpoint for Section 2");
        }
        
        public void TestLoadSection3()
        {
            CheckpointData data = CheckpointManager.Instance.LoadCheckpointProgress(3);
            if (data != null)
                Debug.Log($"✓ Section 3 checkpoint: {data.sceneName} | Gender: {data.gender} | {data.GetTimestampText()}");
            else
                Debug.Log("❌ No checkpoint for Section 3");
        }
        
        // === SCENE LOAD TESTS ===
        
        public void TestGetSceneToLoadSection1()
        {
            string scene = CheckpointManager.Instance.GetSceneToLoad(1);
            Debug.Log($"Section 1 will load: {scene}");
        }
        
        public void TestGetSceneToLoadSection2()
        {
            string scene = CheckpointManager.Instance.GetSceneToLoad(2);
            Debug.Log($"Section 2 will load: {scene}");
        }
        
        public void TestGetSceneToLoadSection3()
        {
            string scene = CheckpointManager.Instance.GetSceneToLoad(3);
            Debug.Log($"Section 3 will load: {scene}");
        }
        
        // === RESET TESTS ===
        
        public void TestResetSection1()
        {
            CheckpointManager.Instance.ResetSectionProgress(1);
            Debug.Log("✓ Section 1 progress reset");
        }
        
        public void TestResetSection2()
        {
            CheckpointManager.Instance.ResetSectionProgress(2);
            Debug.Log("✓ Section 2 progress reset");
        }
        
        public void TestResetSection3()
        {
            CheckpointManager.Instance.ResetSectionProgress(3);
            Debug.Log("✓ Section 3 progress reset");
        }
        
        public void TestResetAll()
        {
            CheckpointManager.Instance.ClearAllCheckpoints();
            Debug.Log("✓ All checkpoints cleared");
        }
        
        // === DEBUG INFO ===
        
        public void TestPrintAll()
        {
            CheckpointManager.Instance.DebugPrintAllCheckpoints();
        }
        
        public void TestHasCheckpoint()
        {
            Debug.Log($"Section 1 has checkpoint: {CheckpointManager.Instance.HasCheckpoint(1)}");
            Debug.Log($"Section 2 has checkpoint: {CheckpointManager.Instance.HasCheckpoint(2)}");
            Debug.Log($"Section 3 has checkpoint: {CheckpointManager.Instance.HasCheckpoint(3)}");
            Debug.Log($"Last played section: {CheckpointManager.Instance.GetLastPlayedSection()}");
        }
        
        // === SCENE NAVIGATION TESTS ===
        
        public void TestNavigateToSection1()
        {
            string scene = CheckpointManager.Instance.GetSceneToLoad(1);
            Debug.Log($"Navigating to Section 1: {scene}");
            SceneSystem.Load(scene);
        }
        
        public void TestNavigateToSection2()
        {
            string scene = CheckpointManager.Instance.GetSceneToLoad(2);
            Debug.Log($"Navigating to Section 2: {scene}");
            SceneSystem.Load(scene);
        }
        
        public void TestNavigateToSection3()
        {
            string scene = CheckpointManager.Instance.GetSceneToLoad(3);
            Debug.Log($"Navigating to Section 3: {scene}");
            SceneSystem.Load(scene);
        }
        
        void OnGUI()
        {
            if (!enableKeyboardShortcuts) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 500));
            GUILayout.Box("=== CHECKPOINT DEBUG PANEL ===");
            
            GUILayout.Label("KEYBOARD SHORTCUTS:");
            GUILayout.Label("1/2/3 = Save Section 1/2/3");
            GUILayout.Label("Q/W/E = Load Section 1/2/3");
            GUILayout.Label("R = Reset All");
            GUILayout.Label("P = Print Debug Info");
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Print All Checkpoints (P)"))
                TestPrintAll();
            
            if (GUILayout.Button("Has Checkpoint?"))
                TestHasCheckpoint();
            
            GUILayout.Space(10);
            GUILayout.Label("--- SAVE TESTS ---");
            
            if (GUILayout.Button("Save Section 1 (1)"))
                TestSaveSection1();
            if (GUILayout.Button("Save Section 2 (2)"))
                TestSaveSection2();
            if (GUILayout.Button("Save Section 3 (3)"))
                TestSaveSection3();
            
            GUILayout.Space(10);
            GUILayout.Label("--- RESET TESTS ---");
            
            if (GUILayout.Button("Reset All (R)"))
                TestResetAll();
            
            GUILayout.EndArea();
        }
    }
}
