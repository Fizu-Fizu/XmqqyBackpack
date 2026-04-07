using UnityEngine;
using XmqqyBackpack;

public class InventoryTestButtons : MonoBehaviour
{
    // 在 Start 中确保数据已加载（如果尚未在其他地方加载）
    private void Start()
    {
        DataManager.LoadAll();
    }

    /// <summary>
    /// 添加5个原木，供按钮调用
    /// </summary>
    public void AddWood()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager 实例不存在！");
            return;
        }

        int left = InventoryManager.Instance.AddItem("Wood", 5);
        Debug.Log($"尝试添加5个原木，剩余未能添加的数量: {left}");
    }

    // 可选：添加其他物品的测试方法
    public void AddStone()
    {
        InventoryManager.Instance?.AddItem("Stone", 5);
    }
}