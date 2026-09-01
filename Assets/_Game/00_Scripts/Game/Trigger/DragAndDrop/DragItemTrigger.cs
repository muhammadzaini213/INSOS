using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class DragItemTrigger : BaseTrigger,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Drag")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private bool canDrag = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onDragStart;
    [SerializeField] private UnityEvent onDragEnd;
    [SerializeField] private UnityEvent onDropSuccess;
    [SerializeField] private UnityEvent onDropFailed;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Transform originalParent;
    private Vector2 originalPosition;

    private bool isDragging;
    private bool dropSuccess;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!canDrag)
            return;

        if (!CanTrigger())
            return;

        isDragging = true;
        dropSuccess = false;

        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;

        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false;

        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();

        onDragStart?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        rectTransform.anchoredPosition +=
            eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        isDragging = false;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (!dropSuccess)
        {
            ReturnToOriginalPosition();
            onDropFailed?.Invoke();
        }

        onDragEnd?.Invoke();

        AddTriggerCount();
    }

    public void SetDropSuccess(Transform dropZone)
    {
        dropSuccess = true;

        transform.SetParent(dropZone);

        rectTransform.anchoredPosition = Vector2.zero;

        onDropSuccess?.Invoke();
    }

    public void ReturnToOriginalPosition()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
    }

    public void SetCanDrag(bool value)
    {
        canDrag = value;
    }

    public bool IsDragging()
    {
        return isDragging;
    }
}