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

        public void Initialize(ItemData data)
        {
            recipeData = data;
            if (recipeData == null || !recipeData.CanCraft) return;

            ClearContainer(materialsContainer);
            ClearContainer(productsContainer);

            AddItemEntry(productsContainer, recipeData.DefName, 1);

            if (recipeData.CraftRecipe != null)
            {
                foreach (CostItem cost in recipeData.CraftRecipe)
                {
                    AddItemEntry(materialsContainer, cost.DefName, cost.Amount);
                }
            }

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
            if (InventoryView.Instance == null)
            {
                Debug.LogError("InventoryView 实例不存在！");
                return;
            }

            bool success = InventoryView.Instance.CraftItem(recipeData.DefName);
            if (success)
            {
                Debug.Log($"合成成功: {recipeData.Label}");
            }
            else
            {
                Debug.Log($"合成失败: {recipeData.Label}，材料不足或其它条件不满足");
            }
        }
    }
}