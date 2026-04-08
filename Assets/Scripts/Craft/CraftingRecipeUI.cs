using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace XmqqyBackpack
{
    public class CraftingRecipeUI : MonoBehaviour
    {
        [Header("UI 组件引用")]
        public Transform materialsContainer;   // 材料列表的父物体（建议带 GridLayoutGroup）
        public Transform productsContainer;    // 产物列表的父物体
        public Button craftButton;             // 合成按钮

        [Header("预制体")]
        public GameObject itemEntryPrefab;     // CraftingSlotUI 预制体（用于材料和产物）

        private ItemData recipeData;           // 当前配方对应的物品数据

        /// <summary>
        /// 初始化配方 UI
        /// </summary>
        public void Initialize(ItemData data)
        {
            recipeData = data;
            if (recipeData == null || !recipeData.CanCraft) return;

            // 清空容器（防止重复生成）
            ClearContainer(materialsContainer);
            ClearContainer(productsContainer);

            // 设置产物列表（通常一个配方产出一种物品，数量固定为1，也可扩展）
            AddItemEntry(productsContainer, recipeData.DefName, 1);

            // 设置材料列表
            if (recipeData.CraftRecipe != null)
            {
                foreach (CostItem cost in recipeData.CraftRecipe)
                {
                    AddItemEntry(materialsContainer, cost.DefName, cost.Amount);
                }
            }

            // 绑定合成按钮事件
            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(OnCraftButtonClicked);
        }

        private void AddItemEntry(Transform container, string defName, int amount)
        {
            GameObject entryObj = Instantiate(itemEntryPrefab, container);
            CraftingSlotUI slotUI = entryObj.GetComponent<CraftingSlotUI>();
            if (slotUI != null)
                slotUI.SetItem(defName, amount);
            else
                Debug.LogError("ItemEntryPrefab 缺少 CraftingSlotUI 组件！");
        }

        private void ClearContainer(Transform container)
        {
            foreach (Transform child in container)
                Destroy(child.gameObject);
        }

        private void OnCraftButtonClicked()
        {
            if (InventoryManager.Instance == null)
            {
                Debug.LogError("InventoryManager 实例不存在！");
                return;
            }

            bool success = InventoryManager.Instance.CraftItem(recipeData.DefName);
            if (success)
            {
                Debug.Log($"合成成功: {recipeData.Label}");
                // 可选：播放音效、UI特效、刷新合成表（如高亮按钮）
            }
            else
            {
                Debug.Log($"合成失败: {recipeData.Label}，材料不足或其它条件不满足");
                // 可选：显示红色提示文字
            }
        }
    }
}