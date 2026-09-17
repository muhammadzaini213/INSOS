using UnityEngine;

/// <summary>
/// Dipanggil via Unity Event dari child.
/// Mencari parent pertama yang punya ChildListenerTrigger, lalu trigger event-nya.
/// </summary>
public class AffectParentTrigger : BaseTrigger
{
    public void AffectParent()
    {
        if (!CanTrigger())
            return;

        AddTriggerCount();

        ChildListenerTrigger listener = GetComponentInParent<ChildListenerTrigger>();
        if (listener != null)
        {
            listener.Trigger();
            return;
        }

        Debug.LogWarning(
            $"[AffectParentTrigger] Tidak ada ChildListenerTrigger di parent hierarchy '{gameObject.name}'."
        );
    }
}
