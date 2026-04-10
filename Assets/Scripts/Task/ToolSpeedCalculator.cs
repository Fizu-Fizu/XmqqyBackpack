using XmqqyBackpack;

/// <summary>
/// 根据建筑需求和背包中匹配的工具计算工作速度加成
/// </summary>
public static class ToolSpeedCalculator
{
    private const float BASE_WORK_SPEED = 1000f; // 【修改】每秒 1000 毫秒工作量

    /// <summary>
    /// 获取拆除指定建筑的工作速度（基础速度 × 最大匹配工具加成）
    /// </summary>
    public static float GetDeconstructWorkSpeed(BuildingData buildingData)
    {
        float bestBonus = 1.0f;
        if (buildingData.DestroyTypes == null || buildingData.DestroyTypes.Count == 0)
            return BASE_WORK_SPEED * bestBonus;

        InventoryManager inv = InventoryManager.Instance;
        if (inv == null) return BASE_WORK_SPEED * bestBonus;

        for (int i = 0; i < 100; i++) // 容量与 InventoryManager.CAPACITY 一致
        {
            InventorySlot slot = inv.GetSlot(i);
            if (slot == null || slot.IsEmpty) continue;

            ItemData item = DataManager.GetItem(slot.ItemDefName);
            if (item?.ToolProperties == null) continue;
            if (item.ToolProperties.DestroyTypes == null) continue;

            foreach (string requiredType in buildingData.DestroyTypes)
            {
                if (item.ToolProperties.DestroyTypes.Contains(requiredType))
                {
                    if (item.ToolProperties.WorkSpeedBonus > bestBonus)
                        bestBonus = item.ToolProperties.WorkSpeedBonus;
                    break;
                }
            }
        }

        return BASE_WORK_SPEED * bestBonus;
    }
}