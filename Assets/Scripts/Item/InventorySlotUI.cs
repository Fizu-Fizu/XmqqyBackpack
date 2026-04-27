using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XmqqyBackpack;

public class InventorySlotUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private GameObject itemContainer;
    [SerializeField] private Image iconImage;
    [SerializeField] private Text amountText;

    public int slotId { get; private set; }
    private bool isDragging = false;
    private bool isLongPress = false;
    private Coroutine longPressCoroutine;
    private static InventorySlotUI currentDraggedSlot;

    public void Initialize(int id)
    {
        slotId = id;
        SetEmpty();
    }

    public void Refresh(InventorySlot slotData)
    {
        if (isDragging) return;

        if (slotData == null || slotData.IsEmpty)
        {
            SetEmpty();
            return;
        }

        ItemData itemData = DataManager.GetItem(slotData.ItemDefName);
        if (itemData == null)
        {
            SetEmpty();
            return;
        }

        itemContainer.SetActive(true);
        if (!string.IsNullOrEmpty(itemData.IconPath))
        {
            Sprite sprite = Resources.Load<Sprite>(itemData.IconPath);
            iconImage.sprite = sprite;
        }
        amountText.text = slotData.Amount.ToString();
    }

    private void SetEmpty()
    {
        itemContainer.SetActive(false);
        iconImage.sprite = null;
        amountText.text = "";
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentDraggedSlot != null) return;

        InventorySlot slot = InventoryView.Instance.GetSlot(slotId);
        if (slot == null || slot.IsEmpty) return;

        longPressCoroutine = StartCoroutine(LongPressDetect(eventData));
    }

    private IEnumerator LongPressDetect(PointerEventData eventData)
    {
        yield return new WaitForSeconds(0.2f);
        isLongPress = true;
        StartDragging(eventData);
    }

    private void StartDragging(PointerEventData eventData)
    {
        if (isDragging) return;
        isDragging = true;
        currentDraggedSlot = this;

        InventorySlot slot = InventoryView.Instance.GetSlot(slotId);
        if (slot == null || slot.IsEmpty)
        {
            CancelDragging();
            return;
        }

        ItemData itemData = DataManager.GetItem(slot.ItemDefName);
        if (itemData == null)
        {
            CancelDragging();
            return;
        }

        itemContainer.SetActive(false);

        Sprite icon = iconImage.sprite;
        if (icon != null)
        {
            DragClone.Instance.Show(icon, slot.Amount, eventData.position);
        }
        else
        {
            Debug.LogWarning($"物品 {slot.ItemDefName} 的图标为空，拖拽时不显示预览");
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        DragClone.Instance.UpdatePosition(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (longPressCoroutine != null)
        {
            StopCoroutine(longPressCoroutine);
            longPressCoroutine = null;
        }

        if (isDragging)
        {
            FinishDragging(eventData);
        }
        else if (!isLongPress)
        {
        }

        isLongPress = false;
    }

    private void FinishDragging(PointerEventData eventData)
    {
        DragClone.Instance.Hide();

        GameObject targetObj = eventData.pointerCurrentRaycast.gameObject;
        int targetId = -1;
        if (targetObj != null)
        {
            InventorySlotUI targetSlotUI = targetObj.GetComponentInParent<InventorySlotUI>();
            if (targetSlotUI != null)
                targetId = targetSlotUI.slotId;
        }

        if (targetId != -1 && targetId != slotId)
        {
            InventoryView.Instance.SwapSlots(slotId, targetId);
        }
        else
        {
            Refresh(InventoryView.Instance.GetSlot(slotId));
        }

        isDragging = false;
        currentDraggedSlot = null;
    }

    private void CancelDragging()
    {
        isDragging = false;
        currentDraggedSlot = null;
        DragClone.Instance.Hide();
        Refresh(InventoryView.Instance.GetSlot(slotId));
    }

    private void OnDisable()
    {
        if (isDragging)
        {
            CancelDragging();
        }
    }
}