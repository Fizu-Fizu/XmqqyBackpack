using System.Collections.Generic;
using UnityEngine;
using XmqqyBackpack;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("UI 配置")]
    [SerializeField] private GameObject slotPrefab;      // 单物品栏预制体
    [SerializeField] private Transform slotsParent;      // 放置格子的父物体

    private const int CAPACITY = 100;
    private InventorySlot[] slots = new InventorySlot[CAPACITY];
    private List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 初始化数据槽位
        for (int i = 0; i < CAPACITY; i++)
        {
            slots[i] = new InventorySlot();
        }

        // 生成UI格子
        for (int i = 0; i < CAPACITY; i++)
        {
            GameObject go = Instantiate(slotPrefab, slotsParent);
            InventorySlotUI slotUI = go.GetComponent<InventorySlotUI>();
            if (slotUI == null)
            {
                Debug.LogError("预制体缺少 InventorySlotUI 组件！");
                continue;
            }
            slotUI.Initialize(i);
            slotUIs.Add(slotUI);
        }

        // 初始刷新全部（默认都是空格）
        RefreshAllUI();
    }

    // ========== 公共接口 ==========

    /// <summary>
    /// 尝试添加物品，返回成功添加的数量（未堆叠完的剩余部分会返回）
    /// </summary>
    public int AddItem(string itemDefName, int amount)
    {
        if (amount <= 0) return 0;

        ItemData itemData = DataManager.GetItem(itemDefName);
        if (itemData == null)
        {
            Debug.LogError($"物品数据不存在: {itemDefName}");
            return amount;
        }

        int maxStack = itemData.MaxStack;
        int remaining = amount;
        List<int> changedIndices = new List<int>();

        // 第一步：尝试堆叠到已有相同物品且未满的格子
        for (int i = 0; i < CAPACITY; i++)
        {
            if (remaining <= 0) break;

            InventorySlot slot = slots[i];
            if (slot.IsEmpty) continue;
            if (slot.ItemDefName != itemDefName) continue;
            if (slot.Amount >= maxStack) continue;

            int canAdd = maxStack - slot.Amount;
            int addAmount = Mathf.Min(canAdd, remaining);
            slot.Amount += addAmount;
            remaining -= addAmount;
            changedIndices.Add(i);
        }

        // 第二步：剩余物品放入空格子
        for (int i = 0; i < CAPACITY; i++)
        {
            if (remaining <= 0) break;

            InventorySlot slot = slots[i];
            if (!slot.IsEmpty) continue;

            int addAmount = Mathf.Min(maxStack, remaining);
            slot.ItemDefName = itemDefName;
            slot.Amount = addAmount;
            remaining -= addAmount;
            changedIndices.Add(i);
        }

        // 刷新受影响的UI
        RefreshSlots(changedIndices);

        return remaining;
    }

    /// <summary>
    /// 移除指定数量的物品（按DefName），返回实际移除的数量
    /// </summary>
    public int RemoveItem(string itemDefName, int amount)
    {
        if (amount <= 0) return 0;

        int remaining = amount;
        List<int> changedIndices = new List<int>();

        // 从后往前遍历，通常没有特殊顺序要求
        for (int i = CAPACITY - 1; i >= 0; i--)
        {
            if (remaining <= 0) break;

            InventorySlot slot = slots[i];
            if (slot.IsEmpty) continue;
            if (slot.ItemDefName != itemDefName) continue;

            int removeAmount = Mathf.Min(slot.Amount, remaining);
            slot.Amount -= removeAmount;
            remaining -= removeAmount;

            if (slot.Amount <= 0)
                slot.Clear();

            changedIndices.Add(i);
        }

        RefreshSlots(changedIndices);
        return amount - remaining;
    }

    /// <summary>
    /// 移除指定格子的物品（按索引）
    /// </summary>
    public bool RemoveItemAt(int slotIndex, int amount)
    {
        if (slotIndex < 0 || slotIndex >= CAPACITY) return false;
        InventorySlot slot = slots[slotIndex];
        if (slot.IsEmpty) return false;
        if (amount <= 0) return false;

        int removeAmount = Mathf.Min(slot.Amount, amount);
        slot.Amount -= removeAmount;
        if (slot.Amount <= 0)
            slot.Clear();

        RefreshSlot(slotIndex);
        return true;
    }

    /// <summary>
    /// 获取格子数据（只读）
    /// </summary>
    public InventorySlot GetSlot(int index)
    {
        if (index < 0 || index >= CAPACITY) return null;
        return slots[index];
    }

    // ========== UI 刷新 ==========
    private void RefreshAllUI()
    {
        for (int i = 0; i < CAPACITY; i++)
        {
            slotUIs[i].Refresh(slots[i]);
        }
    }

    private void RefreshSlot(int index)
    {
        if (index >= 0 && index < slotUIs.Count)
            slotUIs[index].Refresh(slots[index]);
    }

    private void RefreshSlots(List<int> indices)
    {
        foreach (int i in indices)
        {
            RefreshSlot(i);
        }
    }
    
    /// <summary>
    /// 交换两个格子的内容
    /// </summary>
    public void SwapSlots(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= CAPACITY || indexB < 0 || indexB >= CAPACITY)
            return;

        InventorySlot temp = slots[indexA];
        slots[indexA] = slots[indexB];
        slots[indexB] = temp;

        RefreshSlotUI(indexA);
        RefreshSlotUI(indexB);
    }

    /// <summary>
    /// 刷新单个格子的 UI（供外部调用）
    /// </summary>
    public void RefreshSlotUI(int index)
    {
        if (index >= 0 && index < slotUIs.Count)
            slotUIs[index].Refresh(slots[index]);
    }
}