// using System.Collections.Generic;
// using UnityEngine;
// using XmqqyBackpack;
// using UnityEngine.UI;

// [System.Serializable]
// public class SlotSaveData
// {
//     public int slotId;
//     public string itemDefName;
//     public int amount;
// }

// public class InventoryManager : MonoBehaviour
// {
//     public static InventoryManager Instance { get; private set; }

//     [Header("UI 配置")]
//     [SerializeField] private GameObject slotPrefab;      // 单物品栏预制体
//     [SerializeField] private Transform slotsParent;      // 放置格子的父物体

//     // 核心数据结构
//     private Dictionary<int, InventorySlot> slots = new Dictionary<int, InventorySlot>();
//     private List<int> slotIds = new List<int>();
//     private int nextSlotId = 1;

//     // UI 活跃映射
//     private Dictionary<int, InventorySlotUI> activeSlotUIs = new Dictionary<int, InventorySlotUI>();

//     private void Awake()
//     {
//         if (Instance != null)
//         {
//             Destroy(gameObject);
//             return;
//         }
//         Instance = this;
//         DontDestroyOnLoad(gameObject);
//     }

//     private void Start()
//     {
//         // 初始清空父物体下所有旧对象
//         foreach (Transform child in slotsParent)
//         {
//             Destroy(child.gameObject);
//         }
//     }

//     // ========== 辅助私有方法 ==========

//     private int AllocateSlotId()
//     {
//         return nextSlotId++;
//     }

//     private void ClearSlot(int slotId)
//     {
//         if (!slots.ContainsKey(slotId)) return;

//         slots.Remove(slotId);
//         slotIds.Remove(slotId);

//         if (activeSlotUIs.TryGetValue(slotId, out var slotUI))
//         {
//             Destroy(slotUI.gameObject);
//             activeSlotUIs.Remove(slotId);
//         }
//     }

//     // ========== 公共接口 ==========

//     /// <summary>
//     /// 尝试添加物品，返回成功添加的数量（未堆叠完的剩余部分会返回）
//     /// </summary>
//     public int AddItem(string itemDefName, int amount)
//     {
//         if (amount <= 0) return 0;

//         ItemData itemData = DataManager.GetItem(itemDefName);
//         if (itemData == null)
//         {
//             Debug.LogError($"物品数据不存在: {itemDefName}");
//             return amount;
//         }

//         int maxStack = itemData.MaxStack;
//         int remaining = amount;
//         List<int> changedSlotIds = new List<int>();

//         // 第一步：尝试堆叠到已有相同物品且未满的格子
//         foreach (var kvp in slots)
//         {
//             if (remaining <= 0) break;
//             int id = kvp.Key;
//             InventorySlot slot = kvp.Value;
//             if (slot.IsEmpty) continue;
//             if (slot.ItemDefName != itemDefName) continue;
//             if (slot.Amount >= maxStack) continue;

//             int canAdd = maxStack - slot.Amount;
//             int addAmount = Mathf.Min(canAdd, remaining);
//             slot.Amount += addAmount;
//             remaining -= addAmount;
//             changedSlotIds.Add(id);
//         }

//         // 第二步：剩余物品放入新格子
//         while (remaining > 0)
//         {
//             int newId = AllocateSlotId();
//             var newSlot = new InventorySlot();
//             newSlot.ItemDefName = itemDefName;
//             int addAmount = Mathf.Min(maxStack, remaining);
//             newSlot.Amount = addAmount;
//             slots.Add(newId, newSlot);
//             slotIds.Add(newId);
//             remaining -= addAmount;
//             changedSlotIds.Add(newId);

//             // 创建对应的 UI
//             CreateSlotUI(newId);
//         }

//         // 刷新受影响的UI
//         RefreshSlots(changedSlotIds);

//         return remaining;
//     }

//     /// <summary>
//     /// 移除指定数量的物品（按DefName），返回实际移除的数量
//     /// </summary>
//     public int RemoveItem(string itemDefName, int amount)
//     {
//         if (amount <= 0) return 0;

//         int remaining = amount;
//         List<int> changedSlotIds = new List<int>();
//         List<int> toClear = new List<int>();

//         // 倒序遍历 slotIds
//         for (int i = slotIds.Count - 1; i >= 0; i--)
//         {
//             if (remaining <= 0) break;

//             int slotId = slotIds[i];
//             InventorySlot slot = slots[slotId];
//             if (slot.IsEmpty) continue;
//             if (slot.ItemDefName != itemDefName) continue;

//             int removeAmount = Mathf.Min(slot.Amount, remaining);
//             slot.Amount -= removeAmount;
//             remaining -= removeAmount;

//             if (slot.Amount <= 0)
//                 toClear.Add(slotId);
//             else
//                 changedSlotIds.Add(slotId);
//         }

//         // 清理空槽
//         foreach (int id in toClear)
//         {
//             ClearSlot(id);
//         }

//         RefreshSlots(changedSlotIds);
//         return amount - remaining;
//     }

//     /// <summary>
//     /// 移除指定格子的物品（按ID）
//     /// </summary>
//     public bool RemoveItemAt(int slotId, int amount)
//     {
//         if (!slots.ContainsKey(slotId)) return false;
//         InventorySlot slot = slots[slotId];
//         if (slot.IsEmpty) return false;
//         if (amount <= 0) return false;

//         int removeAmount = Mathf.Min(slot.Amount, amount);
//         slot.Amount -= removeAmount;

//         if (slot.Amount <= 0)
//             ClearSlot(slotId);
//         else
//             RefreshSlotUI(slotId);

//         return true;
//     }

//     /// <summary>
//     /// 获取格子数据（只读）
//     /// </summary>
//     public InventorySlot GetSlot(int slotId)
//     {
//         return slots.TryGetValue(slotId, out var s) ? s : null;
//     }

//     // ========== UI 刷新 ==========
//     private void RefreshAllUI()
//     {
//         // 先清空所有现有 UI
//         foreach (Transform child in slotsParent)
//         {
//             Destroy(child.gameObject);
//         }
//         activeSlotUIs.Clear();

//         // 按 slotIds 顺序重新创建所有 UI
//         foreach (int slotId in slotIds)
//         {
//             CreateSlotUI(slotId);
//         }
//     }

//     private void RefreshSlot(int slotId)
//     {
//         RefreshSlotUI(slotId);
//     }

//     private void RefreshSlots(List<int> slotIdsList)
//     {
//         foreach (int id in slotIdsList)
//         {
//             RefreshSlot(id);
//         }
//     }

//     /// <summary>
//     /// 刷新单个格子的 UI（供外部调用）
//     /// </summary>
//     public void RefreshSlotUI(int slotId)
//     {
//         if (activeSlotUIs.TryGetValue(slotId, out var slotUI))
//         {
//             if (slots.TryGetValue(slotId, out var slot))
//                 slotUI.Refresh(slot);
//         }
//     }

//     /// <summary>
//     /// 交换两个格子的内容
//     /// </summary>
//     public void SwapSlots(int idA, int idB)
//     {
//         if (!slots.ContainsKey(idA) || !slots.ContainsKey(idB)) return;

//         InventorySlot temp = slots[idA];
//         slots[idA] = slots[idB];
//         slots[idB] = temp;

//         RefreshSlot(idA);
//         RefreshSlot(idB);
//     }

//     // ========== 合成相关方法 ==========

//     /// <summary>
//     /// 获取背包中指定物品的总数量
//     /// </summary>
//     public int GetTotalAmount(string defName)
//     {
//         int total = 0;
//         foreach (var slot in slots.Values)
//         {
//             if (!slot.IsEmpty && slot.ItemDefName == defName)
//                 total += slot.Amount;
//         }
//         return total;
//     }

//     /// <summary>
//     /// 尝试合成物品（根据 ItemData 中的 CraftRecipe 扣除材料并添加成品）
//     /// </summary>
//     public bool CraftItem(string resultDefName)
//     {
//         ItemData resultData = DataManager.GetItem(resultDefName);
//         if (resultData == null || !resultData.CanCraft || resultData.CraftRecipe == null)
//         {
//             Debug.LogWarning($"无法合成 {resultDefName}：数据无效或不可合成");
//             return false;
//         }

//         // 1. 检查材料是否足够
//         foreach (CostItem cost in resultData.CraftRecipe)
//         {
//             if (GetTotalAmount(cost.DefName) < cost.Amount)
//             {
//                 Debug.Log($"材料不足：需要 {cost.DefName} x{cost.Amount}");
//                 return false;
//             }
//         }

//         List<int> toClear = new List<int>();
//         // 2. 扣除材料
//         foreach (CostItem cost in resultData.CraftRecipe)
//         {
//             int need = cost.Amount;
//             // 倒序遍历
//             for (int i = slotIds.Count - 1; i >= 0 && need > 0; i--)
//             {
//                 int slotId = slotIds[i];
//                 InventorySlot slot = slots[slotId];
//                 if (!slot.IsEmpty && slot.ItemDefName == cost.DefName)
//                 {
//                     int take = Mathf.Min(need, slot.Amount);
//                     slot.Amount -= take;
//                     need -= take;
//                     if (slot.Amount <= 0)
//                         toClear.Add(slotId);
//                 }
//             }
//         }

//         foreach (int id in toClear)
//         {
//             ClearSlot(id);
//         }

//         // 3. 添加成品（默认数量为1，可根据需求扩展）
//         AddItem(resultDefName, 1);
//         RefreshAllUI();
//         Debug.Log($"合成成功：{resultData.Label}");
//         return true;
//     }

//     // ========== UI 创建 ==========
//     private void CreateSlotUI(int slotId)
//     {
//         if (!slots.TryGetValue(slotId, out var slot)) return;

//         GameObject go = Instantiate(slotPrefab, slotsParent);
//         InventorySlotUI slotUI = go.GetComponent<InventorySlotUI>();
//         if (slotUI == null)
//         {
//             Debug.LogError("预制体缺少 InventorySlotUI 组件！");
//             Destroy(go);
//             return;
//         }
//         slotUI.Initialize(slotId);
//         slotUI.Refresh(slot);
//         activeSlotUIs.Add(slotId, slotUI);
//     }

//     // ========== 序列化与存档 ==========
//     public List<SlotSaveData> SaveSlots()
//     {
//         List<SlotSaveData> saveList = new List<SlotSaveData>();
//         foreach (var kvp in slots)
//         {
//             saveList.Add(new SlotSaveData
//             {
//                 slotId = kvp.Key,
//                 itemDefName = kvp.Value.ItemDefName,
//                 amount = kvp.Value.Amount
//             });
//         }
//         return saveList;
//     }

//     public void LoadSlots(List<SlotSaveData> saveList)
//     {
//         slots.Clear();
//         slotIds.Clear();

//         int maxId = 0;
//         foreach (var data in saveList)
//         {
//             var slot = new InventorySlot
//             {
//                 ItemDefName = data.itemDefName,
//                 Amount = data.amount
//             };
//             slots.Add(data.slotId, slot);
//             slotIds.Add(data.slotId);
//             if (data.slotId > maxId) maxId = data.slotId;
//         }

//         nextSlotId = maxId + 1;
//         RefreshAllUI();
//     }
// }