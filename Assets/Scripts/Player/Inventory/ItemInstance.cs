using System;

[System.Serializable]
public class ItemInstance
{
    public ItemData itemData;      // tham chiếu tới ScriptableObject gốc
    public int upgradeLevel = 1;   // cấp nâng trang bị
    public int instanceID;         // id duy nhất (nếu cần)

    // Tạo mới khi player nhặt được item
    public ItemInstance(ItemData data)
    {
        itemData = data;
        upgradeLevel = 1;
        instanceID = GenerateUniqueID();
    }
    
    public ItemInstance(ItemData data, int upgradeLevel, int instanceID = -1)
    {
        itemData = data;
        this.upgradeLevel = upgradeLevel;

        // Nếu load từ save thì dùng lại instanceID cũ,
        // nếu không thì generate cái mới
        this.instanceID = (instanceID == -1) ? GenerateUniqueID() : instanceID;
    }


    public ItemInstanceData ToData()
    {
        return new ItemInstanceData
        {
            itemID = itemData.itemID,
            upgradeLevel = upgradeLevel,
            instanceID = instanceID
        };
    }
    
    private int GenerateUniqueID()
    {
        return Guid.NewGuid().GetHashCode();
    }
}