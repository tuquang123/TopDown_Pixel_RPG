using System;
using UnityEngine;

[Serializable]
public class ItemInstance
{
    public ItemData itemData;

    public bool isLocked; 
    // ⚠️ QUY ƯỚC CHUẨN:
    // level 1 = item gốc, CHƯA nâng cấp
    [Min(1)]
    public int upgradeLevel;

    public int instanceID;

    // =========================
    // CONSTRUCTOR – TẠO ITEM MỚI
    // =========================
    public ItemInstance(ItemData data)
    {
        if (data == null)
        {
            Debug.LogError("ItemInstance: ItemData is null");
            return;
        }
        isLocked = false;
        itemData = data;
        upgradeLevel = 1; // ✅ FIX: level gốc PHẢI là 1
        instanceID = GenerateUniqueID();
    }

    // =========================
    // CONSTRUCTOR – LOAD TỪ SAVE
    // =========================
    public ItemInstance(ItemData data, int loadedUpgradeLevel, int loadedInstanceID = -1, bool locked = false)
    {
        if (data == null)
        {
            Debug.LogError("ItemInstance: ItemData is null");
            return;
        }
        isLocked = locked;
        itemData = data;

        // ✅ FIX: clamp tuyệt đối, không cho < 1
        upgradeLevel = Mathf.Max(1, loadedUpgradeLevel);

        instanceID = (loadedInstanceID <= 0)
            ? GenerateUniqueID()
            : loadedInstanceID;
    }

    // =========================
    // NÂNG CẤP ITEM (SAFE)
    // =========================
    public void Upgrade(int amount = 1)
    {
        upgradeLevel = Mathf.Max(1, upgradeLevel + amount);
    }

    // =========================
    // SET LEVEL TRỰC TIẾP (ADMIN / DEBUG)
    // =========================
    public void SetUpgradeLevel(int level)
    {
        upgradeLevel = Mathf.Max(1, level);
    }

    // =========================
    // SAVE DATA
    // =========================
    public ItemInstanceData ToData()
    {
        return new ItemInstanceData
        {
            itemID = itemData.itemID,
            upgradeLevel = Mathf.Max(1, upgradeLevel), // double safety
            instanceID = instanceID,
            isLocked = isLocked 
        };
    }

    // =========================
    // INTERNAL
    // =========================
    private int GenerateUniqueID()
    {
        return Guid.NewGuid().GetHashCode();
    }
    
}

