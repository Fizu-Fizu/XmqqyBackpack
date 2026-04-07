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

    private int slotIndex;
    private bool isDragging = false;
    private bool isLongPress = false;
    private Coroutine longPressCoroutine;
    private static InventorySlotUI currentDraggedSlot;

    public void Initialize(int index)
    {
        slotIndex = index;
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
        // 修正点：使用 IconPath 而不是 TexturePath 或 ItemPath
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

        InventorySlot slot = InventoryManager.Instance.GetSlot(slotIndex);
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

        InventorySlot slot = InventoryManager.Instance.GetSlot(slotIndex);
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

        // 隐藏原 item
        itemContainer.SetActive(false);

        // 显示拖拽克隆体（仅当图标有效时）
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
            // 普通点击可扩展功能
        }

        isLongPress = false;
    }

    private void FinishDragging(PointerEventData eventData)
    {
        // 1. 隐藏克隆体
        DragClone.Instance.Hide();

        // 2. 获取松开鼠标时射线命中的物体
        GameObject targetObj = eventData.pointerCurrentRaycast.gameObject;
        int targetIndex = -1;
        if (targetObj != null)
        {
            InventorySlotUI targetSlotUI = targetObj.GetComponentInParent<InventorySlotUI>();
            if (targetSlotUI != null)
                targetIndex = targetSlotUI.slotIndex;
        }

        // 3. 执行交换逻辑
        if (targetIndex != -1 && targetIndex != slotIndex)
        {
            InventoryManager.Instance.SwapSlots(slotIndex, targetIndex);
        }
        else
        {
            // 没有有效的目标格子，恢复自身显示
            Refresh(InventoryManager.Instance.GetSlot(slotIndex));
        }

        // 4. 清理拖拽状态
        isDragging = false;
        currentDraggedSlot = null;

        // 5. 确保目标格子UI刷新（如果交换成功，SwapSlots已经刷新了两个格子，这里再调一次无害）
        if (targetIndex != -1)
        {
            InventoryManager.Instance.RefreshSlotUI(targetIndex);
        }
        InventoryManager.Instance.RefreshSlotUI(slotIndex);
    }

    private void CancelDragging()
    {
        isDragging = false;
        currentDraggedSlot = null;
        DragClone.Instance.Hide();
        Refresh(InventoryManager.Instance.GetSlot(slotIndex));
    }

    private void OnDisable()
    {
        if (isDragging)
        {
            CancelDragging();
        }
    }
}