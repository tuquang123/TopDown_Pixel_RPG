using System;

[System.Serializable]
public class ItemInstance
{
    public ItemData itemData;     
    public int upgradeLevel = 0;   
    public int instanceID;       
    
    public ItemInstance(ItemData data)
    {
        itemData = data;
        upgradeLevel = 0;
        instanceID = GenerateUniqueID();
    }
    
    public ItemInstance(ItemData data, int upgradeLevel, int instanceID = -1)
    {
        itemData = data;
        this.upgradeLevel = upgradeLevel;
        
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