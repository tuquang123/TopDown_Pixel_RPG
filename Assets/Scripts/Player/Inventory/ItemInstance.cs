using UnityEngine;

[System.Serializable]
public class ItemInstance
{
    public ItemData Data;
    public int UpgradeLevel = 1;

    public ItemInstance(ItemData data)
    {
        Data = data;
        UpgradeLevel = 1;
    }

    public string Name => $"{Data.itemName} +{UpgradeLevel}";

    public int BaseUpgradeCost => Data.baseUpgradeCost;
    public int UpgradeCost => BaseUpgradeCost * UpgradeLevel;

    public int SellPrice
    {
        get
        {
            float multiplier = 0.6f + (UpgradeLevel * 0.2f);
            return Mathf.RoundToInt(BaseUpgradeCost * multiplier);
        }
    }

    // Các chỉ số tính toán theo cấp độ
    public int AttackPower => Mathf.RoundToInt(Data.attackPower * (1 + 0.1f * (UpgradeLevel - 1)));
    public int Defense => Mathf.RoundToInt(Data.defense * (1 + 0.1f * (UpgradeLevel - 1)));
    public int HealthBonus => Mathf.RoundToInt(Data.healthBonus * (1 + 0.1f * (UpgradeLevel - 1)));
    public int ManaBonus => Mathf.RoundToInt(Data.manaBonus * (1 + 0.1f * (UpgradeLevel - 1)));

    public float CritChance => Data.critChance + (UpgradeLevel - 1) * 1f;
    public float AttackSpeed => Data.attackSpeed + (UpgradeLevel - 1) * 0.05f;
    public float LifeSteal => Data.lifeSteal + (UpgradeLevel - 1) * 0.5f;
    public float MoveSpeed => Data.moveSpeed + (UpgradeLevel - 1) * 0.05f;

    // Thực hiện nâng cấp item
    public void Upgrade()
    {
        UpgradeLevel++;
    }

    // Để so sánh 2 item (optional nếu dùng Dictionary/Set)
    public bool IsSameBaseItem(ItemData otherData)
    {
        return Data != null && Data.itemID == otherData.itemID;
    }
}