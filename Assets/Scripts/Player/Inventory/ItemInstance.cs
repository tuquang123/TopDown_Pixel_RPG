using UnityEngine;
using System;

[Serializable]
public class ItemInstance
{
    public ItemData itemData; // Tham chiếu đến dữ liệu tĩnh
    public int upgradeLevel = 1; // Cấp độ nâng cấp, mặc định là 1
    public int instanceID; // ID duy nhất để phân biệt các bản sao

    public ItemInstance(ItemData data)
    {
        itemData = data;
        instanceID = GenerateUniqueID();
    }

    private int GenerateUniqueID()
    {
        return Guid.NewGuid().GetHashCode(); // Tạo ID duy nhất
    }
}