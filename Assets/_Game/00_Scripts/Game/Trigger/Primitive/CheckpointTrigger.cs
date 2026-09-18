using Slafurry.System.Checkpoint;
using Slafurry.System.Scene;
using UnityEngine;

public class CheckpointTrigger : BaseTrigger
{
    [SerializeField]
    private bool showDebugLogs;

    private string CurrentScene => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

    // ── Save ───────────────────────────────────────────────

    public void SaveCheckpointSection1()
    {
        if (!CanTrigger())
            return;
        CheckpointManager.Instance.SaveCheckpoint(1, CurrentScene);
        AddTriggerCount();
    }

    public void SaveCheckpointSection2()
    {
        if (!CanTrigger())
            return;
        CheckpointManager.Instance.SaveCheckpoint(2, CurrentScene);
        AddTriggerCount();
    }

    public void SaveCheckpointSection3()
    {
        if (!CanTrigger())
            return;
        CheckpointManager.Instance.SaveCheckpoint(3, CurrentScene);
        AddTriggerCount();
    }

    // ── Load ───────────────────────────────────────────────

    public void LoadSection1()
    {
        if (!CanTrigger())
            return;
        string scene = CheckpointManager.Instance.GetSceneToLoad(1);
        SceneSystem.Load(scene);
        AddTriggerCount();
    }

    public void LoadSection2()
    {
        if (!CanTrigger())
            return;
        string scene = CheckpointManager.Instance.GetSceneToLoad(2);
        SceneSystem.Load(scene);
        AddTriggerCount();
    }

    public void LoadSection3()
    {
        if (!CanTrigger())
            return;
        string scene = CheckpointManager.Instance.GetSceneToLoad(3);
        SceneSystem.Load(scene);
        AddTriggerCount();
    }

    // ── Reset ──────────────────────────────────────────────

    public void ResetSection1()
    {
        if (!CanTrigger())
            return;
        CheckpointManager.Instance.ResetSectionProgress(1);
        AddTriggerCount();
    }

    public void ResetSection2()
    {
        if (!CanTrigger())
            return;
        CheckpointManager.Instance.ResetSectionProgress(2);
        AddTriggerCount();
    }

    public void ResetSection3()
    {
        if (!CanTrigger())
            return;
        CheckpointManager.Instance.ResetSectionProgress(3);
        AddTriggerCount();
    }

    // ── Clear All ──────────────────────────────────────────

    public void ClearAllCheckpoints()
    {
        if (!CanTrigger())
            return;
        CheckpointManager.Instance.ClearAllCheckpoints();
        AddTriggerCount();
    }
}
