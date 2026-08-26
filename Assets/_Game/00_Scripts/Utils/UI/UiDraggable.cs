using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Slafurry.Utils.UI
{
    public class UIDraggable : MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [Header("Drag")]
        [SerializeField] private RectTransform target;
        [SerializeField] private Canvas canvas;
        [SerializeField] private string dropKey = "";

        [Header("Options")]
        [SerializeField] private bool returnToOriginalPosition = true;
        [SerializeField] private bool disableRaycastWhileDragging = true;

        [Header("Events")]
        [SerializeField] private UnityEvent onBeginDrag;
        [SerializeField] private UnityEvent onDrag;
        [SerializeField] private UnityEvent onEndDrag;
        [SerializeField] private UnityEvent onDropAccepted;
        [SerializeField] private UnityEvent onDropRejected;

        private CanvasGroup _canvasGroup;
        private RectTransform _originalParent;
        private Vector2 _originalPosition;

        private bool _dropAccepted;

        public string DropKey => dropKey;

        private void Awake()
        {
            if (target == null)
                target = transform as RectTransform;

            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();

            _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _dropAccepted = false;

            _originalParent = target.parent as RectTransform;
            _originalPosition = target.anchoredPosition;

            if (disableRaycastWhileDragging)
                _canvasGroup.blocksRaycasts = false;

            onBeginDrag?.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (canvas == null)
                return;

            target.anchoredPosition +=
                eventData.delta / canvas.scaleFactor;

            onDrag?.Invoke();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (disableRaycastWhileDragging)
                _canvasGroup.blocksRaycasts = true;

            if (!_dropAccepted && returnToOriginalPosition)
            {
                target.anchoredPosition = _originalPosition;
            }

            onEndDrag?.Invoke();
        }

        public void AcceptDrop()
        {
            _dropAccepted = true;
            onDropAccepted?.Invoke();
        }

        public void RejectDrop()
        {
            _dropAccepted = false;
            onDropRejected?.Invoke();
        }

        public void ResetPosition()
        {
            target.anchoredPosition = _originalPosition;
        }
    }
}