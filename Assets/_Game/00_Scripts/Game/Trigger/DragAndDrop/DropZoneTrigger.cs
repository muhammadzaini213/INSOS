using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DropZoneTrigger : BaseTrigger, IDropHandler
{
    [Header("Accepted Items")]
    [SerializeField] private List<DragItemTrigger> acceptedItems =
        new List<DragItemTrigger>();

    [Header("Drop Settings")]
    [SerializeField] private bool snapItemToZone = true;
    [SerializeField] private bool allowMultipleItems = false;
    [SerializeField] private int maxItems = 1;

    [Header("Correct Drop")]
    [SerializeField] private bool destroyDroppedItem = false;

    [Header("Events")]
    [SerializeField] private UnityEvent onDrop;
    [SerializeField] private UnityEvent onCorrectDrop;
    [SerializeField] private UnityEvent onWrongDrop;
    [SerializeField] private UnityEvent onDropRejected;

    private readonly List<DragItemTrigger> placedItems =
        new List<DragItemTrigger>();


    // ========================================
    // DROP
    // ========================================

    public void OnDrop(PointerEventData eventData)
    {
        if (!CanTrigger())
        {
            onDropRejected?.Invoke();
            return;
        }

        GameObject droppedObject = eventData.pointerDrag;

        if (droppedObject == null)
        {
            onDropRejected?.Invoke();
            return;
        }

        DragItemTrigger dragItem =
            droppedObject.GetComponent<DragItemTrigger>();

        if (dragItem == null)
        {
            onDropRejected?.Invoke();
            return;
        }

        HandleDrop(dragItem);
    }


    private void HandleDrop(DragItemTrigger dragItem)
    {
        onDrop?.Invoke();

        if (!CanAcceptMoreItems())
        {
            onDropRejected?.Invoke();
            return;
        }

        if (!IsAcceptedItem(dragItem))
        {
            onWrongDrop?.Invoke();
            return;
        }

        AcceptItem(dragItem);
    }


    // ========================================
    // ACCEPT ITEM
    // ========================================

    private void AcceptItem(DragItemTrigger dragItem)
    {
        if (placedItems.Contains(dragItem))
            return;

        AddTriggerCount();

        onCorrectDrop?.Invoke();

        if (destroyDroppedItem)
        {
            DestroyDroppedItem(dragItem);
            return;
        }

        placedItems.Add(dragItem);

        if (snapItemToZone)
        {
            dragItem.SetDropSuccess(transform);
        }
    }


    private void DestroyDroppedItem(DragItemTrigger dragItem)
    {
        dragItem.SetDropSuccess(transform);

        placedItems.Remove(dragItem);

        Destroy(dragItem.gameObject);
    }


    // ========================================
    // VALIDATION
    // ========================================

    private bool IsAcceptedItem(DragItemTrigger dragItem)
    {
        return acceptedItems.Contains(dragItem);
    }


    private bool CanAcceptMoreItems()
    {
        if (!allowMultipleItems)
        {
            return placedItems.Count == 0;
        }

        return placedItems.Count < maxItems;
    }


    // ========================================
    // ACCEPTED ITEMS
    // ========================================

    public void AddAcceptedItem(DragItemTrigger item)
    {
        if (item == null)
            return;

        if (!acceptedItems.Contains(item))
        {
            acceptedItems.Add(item);
        }
    }


    public void RemoveAcceptedItem(DragItemTrigger item)
    {
        if (item == null)
            return;

        acceptedItems.Remove(item);
    }


    public void ClearAcceptedItems()
    {
        acceptedItems.Clear();
    }


    public bool IsAccepted(DragItemTrigger item)
    {
        return item != null && acceptedItems.Contains(item);
    }


    // ========================================
    // PLACED ITEMS
    // ========================================

    public bool HasItem(DragItemTrigger item)
    {
        return placedItems.Contains(item);
    }


    public void RemovePlacedItem(DragItemTrigger item)
    {
        if (item == null)
            return;

        placedItems.Remove(item);
    }


    public void ClearPlacedItems()
    {
        placedItems.Clear();
    }


    public int GetPlacedItemCount()
    {
        return placedItems.Count;
    }


    public List<DragItemTrigger> GetPlacedItems()
    {
        return new List<DragItemTrigger>(placedItems);
    }


    public List<DragItemTrigger> GetAcceptedItems()
    {
        return new List<DragItemTrigger>(acceptedItems);
    }
}