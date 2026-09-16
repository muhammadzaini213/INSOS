using Slafurry.System.Audio;
using UnityEngine;

public class SFXTrigger : BaseTrigger
{

    [Header("Audio Data")]
    [SerializeField] private string category;
    [SerializeField] private string key;

    public void PlaySfx()
    {
        if (!CanTrigger()) return;
        Audio.PlaySFX2D(category, key);
        AddTriggerCount();
    }

    public void StopSFX()
    {
        if (!CanTrigger()) return;
        if (!string.IsNullOrEmpty(key)) Audio.StopSFX(category, key);
        else if (!string.IsNullOrEmpty(category)) Audio.StopSFX(category);
        AddTriggerCount();
    }

    public void StopAllSFX()
    {
        if (!CanTrigger()) return;
        Audio.StopSFX();
        AddTriggerCount();
    }
}