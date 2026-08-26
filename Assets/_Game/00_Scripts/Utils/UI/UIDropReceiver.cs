using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Slafurry.Utils.UI
{
    public class UIDropReceiver : MonoBehaviour, IDropHandler
    {
        [Header("Drop")]
        [SerializeField] private string acceptedDropKey = "";

        [Header("Events")]
        [SerializeField] private UnityEvent onDrop;
        [SerializeField] private UnityEvent onDropAccepted;
        [SerializeField] private UnityEvent onDropRejected;

        public void OnDrop(PointerEventData eventData)
        {
            UIDraggable draggable =
                eventData.pointerDrag?.GetComponent<UIDraggable>();

            if (draggable == null)
            {
                onDropRejected?.Invoke();
                return;
            }

            onDrop?.Invoke();

            if (!IsAccepted(draggable))
            {
                draggable.RejectDrop();
                onDropRejected?.Invoke();
                return;
            }

            draggable.AcceptDrop();

            onDropAccepted?.Invoke();
        }

        private bool IsAccepted(UIDraggable draggable)
        {
            // Kosong = menerima semua draggable.
            if (string.IsNullOrEmpty(acceptedDropKey))
                return true;

            return draggable.DropKey == acceptedDropKey;
        }
    }
}