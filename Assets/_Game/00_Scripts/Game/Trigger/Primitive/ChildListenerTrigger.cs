using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Ditempel di parent.
/// Menyimpan UnityEvent yang bisa di-trigger oleh child melalui AffectParentTrigger.
/// </summary>
public class ChildListenerTrigger : BaseTrigger
{
    [SerializeField]
    private UnityEvent onTriggered;

    public void Trigger()
    {
        if (!CanTrigger())
            return;

        AddTriggerCount();
        onTriggered?.Invoke();
    }
}
