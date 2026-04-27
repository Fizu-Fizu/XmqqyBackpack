using System.Collections.Generic;
using System;
using XmqqyBackpack;

[System.Serializable]
public class SlotSaveData
{
    public int slotId;
    public string itemDefName;
    public int amount;
}

public class InventoryModel
{
    private Dictionary<int, InventorySlot> slots = new Dictionary<int, InventorySlot>();
    private List<int> slotIds = new List<int>();
    private int nextSlotId = 1;

    public event Action<List<int>> OnSlotsChanged;

    public IReadOnlyList<int> SlotIds => slotIds;

    // ========== 内部辅助方法 ==========

    private int AllocateSlotId()
    {
        return nextSlotId++;
    }

    private void ClearSlotData(int slotId)
    {
        if (!slots.ContainsKey(slotId)) return;
        slots.Remove(slotId);
        slotIds.Remove(slotId);
    }

    private void NotifyChanged(List<int> changedIds)
    {
        if (changedIds != null && changedIds.Count > 0 && OnSlotsChanged != null)
        {
            OnSlotsChanged.Invoke(changedIds);
        }
    }

    // ========== 公共方法 ==========

    public int AddItem(string itemDefName, int amount)
    {
        if (amount <= 0) return 0;

        ItemData itemData = DataManager.GetItem(itemDefName);
        if (itemData == null)
        {
            UnityEngine.Debug.LogError($"物品数据不存在: {itemDefName}");
            return amount;
        }

        int maxStack = itemData.MaxStack;
        int remaining = amount;
        List<int> changedSlotIds = new List<int>();

        foreach (var kvp in slots)
        {
            if (remaining <= 0) break;
            int id = kvp.Key;
            InventorySlot slot = kvp.Value;
            if (slot.IsEmpty) continue;
            if (slot.ItemDefName != itemDefName) continue;
            if (slot.Amount >= maxStack) continue;

            int canAdd = maxStack - slot.Amount;
            int addAmount = Math.Min(canAdd, remaining);
            slot.Amount += addAmount;
            remaining -= addAmount;
            changedSlotIds.Add(id);
        }

        while (remaining > 0)
        {
            int newId = AllocateSlotId();
            var newSlot = new InventorySlot();
            newSlot.ItemDefName = itemDefName;
            int addAmount = Math.Min(maxStack, remaining);
            newSlot.Amount = addAmount;
            slots.Add(newId, newSlot);
            slotIds.Add(newId);
            remaining -= addAmount;
            changedSlotIds.Add(newId);
        }

        NotifyChanged(changedSlotIds);
        return remaining;
    }

    public int RemoveItem(string itemDefName, int amount)
    {
        if (amount <= 0) return 0;

        int remaining = amount;
        List<int> changedSlotIds = new List<int>();
        List<int> toClear = new List<int>();

        for (int i = slotIds.Count - 1; i >= 0; i--)
        {
            if (remaining <= 0) break;
            int slotId = slotIds[i];
            InventorySlot slot = slots[slotId];
            if (slot.IsEmpty) continue;
            if (slot.ItemDefName != itemDefName) continue;

            int removeAmount = Math.Min(slot.Amount, remaining);
            slot.Amount -= removeAmount;
            remaining -= removeAmount;

            if (slot.Amount <= 0)
            {
                toClear.Add(slotId);
                changedSlotIds.Add(slotId);
            }
            else
            {
                changedSlotIds.Add(slotId);
            }
        }

        foreach (int id in toClear)
        {
            ClearSlotData(id);
        }

        NotifyChanged(changedSlotIds);
        return amount - remaining;
    }

    public bool RemoveItemAt(int slotId, int amount)
    {
        if (!slots.ContainsKey(slotId)) return false;
        InventorySlot slot = slots[slotId];
        if (slot.IsEmpty) return false;
        if (amount <= 0) return false;

        List<int> changedSlotIds = new List<int> { slotId };
        int removeAmount = Math.Min(slot.Amount, amount);
        slot.Amount -= removeAmount;

        if (slot.Amount <= 0)
        {
            ClearSlotData(slotId);
        }

        NotifyChanged(changedSlotIds);
        return true;
    }

    public InventorySlot GetSlot(int slotId)
    {
        return slots.TryGetValue(slotId, out var s) ? s : null;
    }

    public void SwapSlots(int idA, int idB)
    {
        if (!slots.ContainsKey(idA) || !slots.ContainsKey(idB)) return;

        InventorySlot temp = slots[idA];
        slots[idA] = slots[idB];
        slots[idB] = temp;

        NotifyChanged(new List<int> { idA, idB });
    }

    public int GetTotalAmount(string defName)
    {
        int total = 0;
        foreach (var slot in slots.Values)
        {
            if (!slot.IsEmpty && slot.ItemDefName == defName)
                total += slot.Amount;
        }
        return total;
    }

    public bool CraftItem(string resultDefName)
    {
        ItemData resultData = DataManager.GetItem(resultDefName);
        if (resultData == null || !resultData.CanCraft || resultData.CraftRecipe == null)
        {
            UnityEngine.Debug.LogWarning($"无法合成 {resultDefName}：数据无效或不可合成");
            return false;
        }

        foreach (CostItem cost in resultData.CraftRecipe)
        {
            if (GetTotalAmount(cost.DefName) < cost.Amount)
            {
                UnityEngine.Debug.Log($"材料不足：需要 {cost.DefName} x{cost.Amount}");
                return false;
            }
        }

        List<int> changedSlotIds = new List<int>();
        List<int> toClear = new List<int>();

        foreach (CostItem cost in resultData.CraftRecipe)
        {
            int need = cost.Amount;
            for (int i = slotIds.Count - 1; i >= 0 && need > 0; i--)
            {
                int slotId = slotIds[i];
                InventorySlot slot = slots[slotId];
                if (!slot.IsEmpty && slot.ItemDefName == cost.DefName)
                {
                    int take = Math.Min(need, slot.Amount);
                    slot.Amount -= take;
                    need -= take;
                    changedSlotIds.Add(slotId);
                    if (slot.Amount <= 0)
                        toClear.Add(slotId);
                }
            }
        }

        foreach (int id in toClear)
        {
            ClearSlotData(id);
        }

        NotifyChanged(changedSlotIds);

        AddItem(resultDefName, 1);
        UnityEngine.Debug.Log($"合成成功：{resultData.Label}");
        return true;
    }

    public List<SlotSaveData> SaveSlots()
    {
        List<SlotSaveData> saveList = new List<SlotSaveData>();
        foreach (var kvp in slots)
        {
            saveList.Add(new SlotSaveData
            {
                slotId = kvp.Key,
                itemDefName = kvp.Value.ItemDefName,
                amount = kvp.Value.Amount
            });
        }
        return saveList;
    }

    public void LoadSlots(List<SlotSaveData> saveList)
    {
        slots.Clear();
        slotIds.Clear();

        int maxId = 0;
        foreach (var data in saveList)
        {
            var slot = new InventorySlot
            {
                ItemDefName = data.itemDefName,
                Amount = data.amount
            };
            slots.Add(data.slotId, slot);
            slotIds.Add(data.slotId);
            if (data.slotId > maxId) maxId = data.slotId;
        }

        nextSlotId = maxId + 1;
        NotifyChanged(new List<int>(slotIds));
    }
}
