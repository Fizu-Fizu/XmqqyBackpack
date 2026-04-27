using UnityEngine;
using XmqqyBackpack;

public class InventoryTestButtons : MonoBehaviour
{
    // 随机添加的物品列表
    private static readonly string[] ToolItems = new string[] {
        "Iron_Sword", "Iron_Pickaxe", "Iron_Hoe", "Iron_Axe",
        "Steel_Sword", "Steel_Pickaxe", "Steel_Hoe", "Steel_Axe"
    };

    private void Start()
    {
        DataManager.LoadAll();
    }

    public void AddWood()
    {
        if (InventoryView.Instance == null)
        {
            Debug.LogError("InventoryView 实例不存在！");
            return;
        }

        int left = InventoryView.Instance.AddItem("Wood", 5);
        Debug.Log($"尝试添加5个原木，剩余未能添加的数量: {left}");
    }

    public void AddRandomTool()
    {
        if (InventoryView.Instance == null) return;

        // 随机选一个物品
        int randomIndex = Random.Range(0, ToolItems.Length);
        string randomItemDefName = ToolItems[randomIndex];

        // 随机添加 1-5 个
        int addCount = Random.Range(1, 6);
        int left = InventoryView.Instance.AddItem(randomItemDefName, addCount);
        Debug.Log($"尝试添加{addCount}个{randomItemDefName}，剩余未能添加的数量: {left}");
    }
    public void AddMultipleRandomTools()
    {
        int count = 100;
        for (int i = 0; i < count; i++)
        {
            AddRandomTool();
        }
    }
}