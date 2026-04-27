using XmqqyBackpack;

public static class ToolSpeedCalculator
{
    private const float BASE_WORK_SPEED = 1000f;

    public static float GetDeconstructWorkSpeed(BuildingData buildingData)
    {
        float bestBonus = 1.0f;
        if (buildingData.DestroyTypes == null || buildingData.DestroyTypes.Count == 0)
            return BASE_WORK_SPEED * bestBonus;

        InventoryView inv = InventoryView.Instance;
        if (inv == null) return BASE_WORK_SPEED * bestBonus;

        foreach (int slotId in inv.SlotIds)
        {
            InventorySlot slot = inv.GetSlot(slotId);
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