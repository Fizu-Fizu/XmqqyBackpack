[System.Serializable]
public class InventorySlot
{
    public string ItemDefName;   // 为空表示空格子
    public int Amount;
    
    public bool IsEmpty => string.IsNullOrEmpty(ItemDefName) || Amount <= 0;

    public void Clear()
    {
        ItemDefName = null;
        Amount = 0;
        
    }
}
