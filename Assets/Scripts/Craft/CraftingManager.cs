using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace XmqqyBackpack
{
    public class CraftingManager : MonoBehaviour
    {
        [Header("UI 设置")]
        public GameObject recipePrefab;        // CraftingRecipeUI 预制体
        public Transform recipesParent;        // 配方列表的父物体（建议带 VerticalLayoutGroup + ScrollRect）

        private void Start()
        {
            // 确保数据已加载
            DataManager.LoadAll();
            GenerateAllRecipes();
        }

        /// <summary>
        /// 扫描所有可合成物品，动态生成配方 UI
        /// </summary>
        private void GenerateAllRecipes()
        {
            // 清空原有内容
            foreach (Transform child in recipesParent)
                Destroy(child.gameObject);

            // 获取所有 CanCraft = true 的物品
            var craftableItems = DataManager.GetAllItems().Where(item => item.CanCraft);
            foreach (var item in craftableItems)
            {
                GameObject recipeObj = Instantiate(recipePrefab, recipesParent);
                CraftingRecipeUI recipeUI = recipeObj.GetComponent<CraftingRecipeUI>();
                if (recipeUI != null)
                {
                    recipeUI.Initialize(item);
                }
                else
                {
                    Debug.LogError($"RecipePrefab 缺少 CraftingRecipeUI 组件！预制体路径: {recipePrefab.name}");
                }
            }

            Debug.Log($"合成表生成完毕，共 {craftableItems.Count()} 个配方");
        }

        // 可选：提供手动刷新接口（例如当添加新配方时）
        public void RefreshCraftingUI()
        {
            GenerateAllRecipes();
        }
    }
}