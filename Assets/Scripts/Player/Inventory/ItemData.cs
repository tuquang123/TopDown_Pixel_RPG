using UnityEngine;
using UnityEngine.Serialization;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;

#region Support Types

[Serializable]
[InlineProperty]
[HideLabel]
public class ItemStatBonus
{
    [HorizontalGroup("Bonus", width: 0.5f)] [LabelText("Flat")] [LabelWidth(35)] [GUIColor(0.9f, 0.95f, 1f)]
    public float flat;

    [HorizontalGroup("Bonus", width: 0.5f)] [LabelText("%")] [LabelWidth(25)] [GUIColor(0.95f, 1f, 0.95f)]
    public float percent;

    public static readonly ItemStatBonus Zero = new ItemStatBonus(0f, 0f);

    public ItemStatBonus(float flat = 0, float percent = 0)
    {
        this.flat = flat;
        this.percent = percent;
    }

    public bool HasValue => flat != 0 || percent != 0;

    public override string ToString()
    {
        string result = "";
        if (flat != 0) result += $"+{flat}";
        if (percent != 0)
        {
            if (flat != 0) result += " + ";
            result += $"{percent}%";
        }

        return result;
    }
}

public enum ItemType
{
    Weapon,
    Clother,
    Consumable,
    Helmet,
    Boots,
    Horse,
    SpecialArmor,
    Cloak,
    Hair
}

public enum WeaponCategory
{
    Melee,   // Kiếm, chùy, giáo, dao…
    Ranged,   //  shuriken…
    HeavyMelee,
    Bow, // Axe , Hammer
}

public enum ItemTier
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Mythic
}

public static class ItemUtility
{
    private static readonly Dictionary<string, string> VietnameseItemNames = new(StringComparer.OrdinalIgnoreCase)
    {
        { "'Pboosts '", "Gi\u00e0y T\u0103ng T\u1ed1c" },
        { "boostsWhite", "Gi\u00e0y Tr\u1eafng" },
        { "demonhide boosts", "Gi\u00e0y Da Qu\u1ef7" },
        { "dressshoes", "Gi\u00e0y L\u1ec5 Ph\u1ee5c" },
        { "boosts2", "Gi\u00e0y X\u00e1m" },
        { "boosts6", "Gi\u00e0y Xanh L\u00e1" },
        { "iron boosts", "Gi\u00e0y S\u1eaft" },
        { "boosts8", "Gi\u00e0y Kaki Nh\u1ea1t" },
        { "boosts7", "Gi\u00e0y \u0110\u1ecf" },
        { "explorerboots", "Gi\u00e0y Th\u00e1m Hi\u1ec3m" },
        { "boosts3", "Gi\u00e0y T\u00edm" },
        { "slippers", "D\u00e9p L\u00ea" },
        { "dragon boosts", "Gi\u00e0y R\u1ed3ng" },
        { "boosts1", "Gi\u00e0y Da Xanh" },
        { "boosts5", "Gi\u00e0y Tr\u1eafng" },
        { "cape2", "\u00c1o Cho\u00e0ng B\u00f3ng T\u1ed1i" },
        { "cape5", "\u00c1o Cho\u00e0ng Cung Th\u1ee7" },
        { "cape", "\u00c1o Cho\u00e0ng Hi\u1ec7p S\u0129" },
        { "cape3", "Kh\u0103n Cho\u00e0ng Da" },
        { "cape1", "Kh\u0103n Cho\u00e0ng \u0110\u1ecf" },
        { "cloth armor", "\u00c1o Gi\u00e1p V\u1ea3i" },
        { "armor4", "\u00c1o Gi\u00e1p \u0110\u1ed3ng" },
        { "frostarmor", "\u00c1o Gi\u00e1p B\u0103ng Gi\u00e1" },
        { "armor2", "Gi\u00e1p Chi\u1ebfn H\u1ecfa Ng\u1ee5c" },
        { "armor1", "\u00c1o Gi\u00e1p S\u1eaft" },
        { "armor3", "\u00c1o Gi\u00e1p \u0110\u1ea1i D\u01b0\u01a1ng" },
        { "clockworkarmor", "\u00c1o Gi\u00e1p C\u01a1 Kh\u00ed" },
        { "bone armor", "\u00c1o Gi\u00e1p X\u01b0\u01a1ng" },
        { "armor6", "\u00c1o Gi\u00e1p Ng\u1ecdc L\u1ee5c B\u1ea3o" },
        { "armor7", "\u00c1o Gi\u00e1p L\u1eeda" },
        { "lllusionrobe", "\u00c1o Cho\u00e0ng \u1ea2o \u1ea2nh" },
        { "armor8", "\u00c1o Gi\u00e1p Da" },
        { "armor5", "\u00c1o Gi\u00e1p B\u1ea1c" },
        { "hpc", "B\u00ecnh M\u00e1u" },
        { "helmet", "M\u0169 Gi\u00e1p" },
        { "helmet10", "M\u0169 L\u00ednh S\u1eaft" },
        { "robber's cap", "M\u0169 Tr\u1ed9m" },
        { "helmet1", "M\u0169 B\u1ea1c" },
        { "helmet4", "M\u0169 H\u1ed9 V\u1ec7" },
        { "helmet2", "M\u0169 Gai" },
        { "trihorn", "M\u0169 Ba S\u1eebng" },
        { "helmet8", "M\u0169 N\u1eb7ng" },
        { "knight's helmet", "M\u0169 Hi\u1ec7p S\u0129" },
        { "helmet3", "M\u0169 Chi\u1ebfn Binh" },
        { "helmet7", "M\u0169 Tai Th\u1ecf VIP" },
        { "royal helmet", "M\u0169 Ho\u00e0ng Gia" },
        { "helmet5", "M\u0169 \u0110\u1ed3ng" },
        { "horned helmet", "M\u0169 S\u1eebng" },
        { "helmet6", "M\u0169 Ph\u00f9 Th\u1ee7y" },
        { "hourse1", "Chi\u1ebfn M\u00e3" },
        { "hourse4", "Ng\u1ef1a \u0110\u1ecba Ng\u1ee5c" },
        { "hourse", "Ng\u1ef1a" },
        { "hourse2", "Ng\u1ef1a Tr\u1eafng" },
        { "armorsuper3", "\u00c1o Gi\u00e1p Th\u01b0\u1eddng" },
        { "armorsuper2", "\u00c1o Gi\u00e1p Chi\u1ebfn Binh" },
        { "armorsupers", "\u00c1o Gi\u00e1p Nh\u00e0 Vua" },
        { "armorsuper", "Si\u00eau Gi\u00e1p Chi\u1ebfn \u0110\u1ea5u" },
        { "armorsuper6", "Gi\u00e1p Ph\u00f2ng Th\u1ee7" },
        { "armorsuper1", "\u00c1o Gi\u00e1p Th\u00e2n" },
        { "armorsuper8", "Th\u1eaft L\u01b0ng Da" },
        { "dragongay", "\u00c1o Gi\u00e1p R\u1ed3ng VIP" },
        { "iron armor", "\u00c1o Gi\u00e1p S\u1eaft" },
        { "armorsuper7", "\u00c1o Gi\u00e1p Th\u01b0\u1eddng" },
        { "armorsuper5", "\u00c1o Gi\u00e1p Da" },
        { "armorsuper4", "Th\u1eaft L\u01b0ng" },
        { "axe1", "R\u00ecu 1" },
        { "lance2", "Th\u01b0\u01a1ng" },
        { "pig-sticking", "Gi\u00e1o S\u0103n" },
        { "swordshort", "Ki\u1ebfm Ng\u1eafn" },
        { "sword", "Ki\u1ebfm S\u1eaft" },
        { "sword+1", "Ki\u1ebfm S\u1eaft" },
        { "axe2", "R\u00ecuu" },
        { "sword24", "Phi Ti\u00eau C2" },
        { "Bowtitan", "Cung Titan" },
        { "axe3", "R\u00ecu" },
        { "dagger", "Dao G\u0103m" },
        { "dark sword", "H\u1eafc Ki\u1ebfm" },
        { "lance1", "Th\u01b0\u01a1ng" },
        { "lance3", "Th\u01b0\u01a1ng X\u1ecbn" },
        { "sword2", "Phi Ti\u00eau" },
        { "dark sword 2", "H\u1eafc Ki\u1ebfm" },
        { "hammer", "B\u00faa" },
        { "sword5", "Ki\u1ebfm Nh\u00e0 Vua" },
        { "Bowpro", "Cung X\u1ecbn" },
        { "sword4", "Ki\u1ebfm G\u00e3y" },
        { "sword3", "H\u00e0n Phong Ki\u1ebfm" },
        { "lance", "Th\u01b0\u01a1ng" },
        { "lanceoh", "Th\u01b0\u01a1ng" },
        { "sword21", "Phi Ti\u00eau" },
        { "sword23", "Phi Ti\u00eau" },
        { "swordshort1", "Ki\u1ebfm Ng\u1eafn" },
        { "Bow", "Cung" },
        { "sword a", "Ki\u1ebfm Th\u01b0\u1eddng" },
    };

    public static Color GetColorByTier(ItemTier tier)
    {
        return tier switch
        {
            ItemTier.Common    => HexToColor("#707070"), // sáng hơn rõ rệt
            ItemTier.Uncommon  => HexToColor("#2E8E52"), // xanh tươi hơn
            ItemTier.Rare      => HexToColor("#1F64B5"), // xanh dương sáng rõ
            ItemTier.Epic      => HexToColor("#6A44A3"), // tím epic sáng
            ItemTier.Legendary => HexToColor("#9A6A19"), // vàng-cam legendary
            ItemTier.Mythic    => HexToColor("#8B2031"), // đỏ tím mạnh
            _                  => Color.white,
        };
    }

    private static Color HexToColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out var color))
            return color;
        return Color.white;
    }

    public static string GetLocalizedTier(ItemTier tier)
    {
        if (LanguageManager.Instance == null || LanguageManager.Instance.CurrentLanguage != "vi")
            return tier.ToString();

        return tier switch
        {
            ItemTier.Common => "Th\u01b0\u1eddng",
            ItemTier.Uncommon => "Kh\u00f4ng Ph\u1ed5 Bi\u1ebfn",
            ItemTier.Rare => "Hi\u1ebfm",
            ItemTier.Epic => "S\u1eed Thi",
            ItemTier.Legendary => "Huy\u1ec1n Tho\u1ea1i",
            ItemTier.Mythic => "Th\u1ea7n Tho\u1ea1i",
            _ => tier.ToString(),
        };
    }

    public static string GetLocalizedItemName(string itemId, string fallbackName)
    {
        if (LanguageManager.Instance == null || LanguageManager.Instance.CurrentLanguage != "vi")
            return fallbackName;

        if (!string.IsNullOrWhiteSpace(itemId) && VietnameseItemNames.TryGetValue(itemId.Trim(), out string byId))
            return byId;

        if (!string.IsNullOrWhiteSpace(fallbackName) && VietnameseItemNames.TryGetValue(fallbackName.Trim(), out string byName))
            return byName;

        return TranslateGenericItemName(fallbackName);
    }

    private static string TranslateGenericItemName(string fallbackName)
    {
        if (string.IsNullOrWhiteSpace(fallbackName))
            return fallbackName;

        string text = fallbackName.Trim();
        if (text.StartsWith("hair", StringComparison.OrdinalIgnoreCase))
            return "T\u00f3c " + text.Substring(4).Trim();

        string result = text;
        result = ReplaceWord(result, "Iron", "S\u1eaft");
        result = ReplaceWord(result, "Silver", "B\u1ea1c");
        result = ReplaceWord(result, "Bronze", "\u0110\u1ed3ng");
        result = ReplaceWord(result, "Dark", "H\u1eafc");
        result = ReplaceWord(result, "Broken", "G\u00e3y");
        result = ReplaceWord(result, "Cold", "L\u1ea1nh");
        result = ReplaceWord(result, "King's", "Nh\u00e0 Vua");
        result = ReplaceWord(result, "King", "Vua");
        result = ReplaceWord(result, "Dragon", "R\u1ed3ng");
        result = ReplaceWord(result, "Fire", "L\u1eeda");
        result = ReplaceWord(result, "Frost", "B\u0103ng");
        result = ReplaceWord(result, "Leather", "Da");
        result = ReplaceWord(result, "Cloth", "V\u1ea3i");
        result = ReplaceWord(result, "Sword", "Ki\u1ebfm");
        result = ReplaceWord(result, "Axe", "R\u00ecu");
        result = ReplaceWord(result, "Bow", "Cung");
        result = ReplaceWord(result, "Lance", "Th\u01b0\u01a1ng");
        result = ReplaceWord(result, "Dagger", "Dao G\u0103m");
        result = ReplaceWord(result, "Hammer", "B\u00faa");
        result = ReplaceWord(result, "Armor", "Gi\u00e1p");
        result = ReplaceWord(result, "Helmet", "M\u0169");
        result = ReplaceWord(result, "Helm", "M\u0169");
        result = ReplaceWord(result, "Shoes", "Gi\u00e0y");
        result = ReplaceWord(result, "Boots", "Gi\u00e0y");
        result = ReplaceWord(result, "Horse", "Ng\u1ef1a");

        return result;
    }

    private static string ReplaceWord(string source, string from, string to)
    {
        return source
            .Replace(from, to)
            .Replace(from.ToLowerInvariant(), to)
            .Replace(from.ToUpperInvariant(), to);
    }

    public static string GetLocalizedDescription(string itemId, string fallbackDescription)
    {
        if (LanguageManager.Instance == null || LanguageManager.Instance.CurrentLanguage != "vi")
            return fallbackDescription;

        if (string.IsNullOrWhiteSpace(fallbackDescription))
            return fallbackDescription;

        string text = fallbackDescription.Trim();
        if (text.Equals("up speed", StringComparison.OrdinalIgnoreCase))
            return "T\u0103ng t\u1ed1c \u0111\u1ed9.";

        if (text.Equals("Grants enhanced power through demonic leather", StringComparison.OrdinalIgnoreCase))
            return "T\u0103ng s\u1ee9c m\u1ea1nh nh\u1edd da qu\u1ef7.";

        return text;
    }
}







#endregion

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [BoxGroup("General Info")] [LabelWidth(100)]
    public string itemID;

    [BoxGroup("General Info")] [LabelWidth(100)]
    [FormerlySerializedAs("itemName")]
    [SerializeField] private string _itemName;
    public string itemName
    {
        get
        {
            if (LanguageManager.Instance != null && !string.IsNullOrEmpty(itemID))
            {
                string translated = LanguageManager.Instance.GetTranslation(itemID);
                if (!string.IsNullOrWhiteSpace(translated) && translated != itemID)
                    return translated;
            }

            return ItemUtility.GetLocalizedItemName(itemID, _itemName);
        }
        set => _itemName = value;
    }

    [BoxGroup("General Info")] [PreviewField(60, ObjectFieldAlignment.Left)] [HideLabel]
    public Sprite icon;

    [BoxGroup("Weapon Settings")]
    public WeaponCategory weaponCategory;

    [BoxGroup("General Info")] [LabelWidth(100)]
    public ItemType itemType;

    [BoxGroup("General Info")] [LabelWidth(100)]
    public ItemTier tier;

    [BoxGroup("General Info")] [LabelWidth(100)]
    public int price;

    [BoxGroup("General Info")] [MultiLineProperty(3)]
    [FormerlySerializedAs("description")]
    [SerializeField] private string _description;
    public string description
    {
        get
        {
            if (LanguageManager.Instance != null && !string.IsNullOrEmpty(itemID))
            {
                string key = itemID + "_des";
                string translated = LanguageManager.Instance.GetTranslation(key);
                if (!string.IsNullOrWhiteSpace(translated) && translated != key)
                    return translated;
            }

            return ItemUtility.GetLocalizedDescription(itemID, BuildContextualDescription());
        }
        set => _description = value;
    }

    [BoxGroup("Stats"), HideLabel] [FoldoutGroup("Stats/Battle Stats")] [LabelText("ATK")]
    public ItemStatBonus attack = new ItemStatBonus();

    [FoldoutGroup("Stats/Battle Stats")] [LabelText("DEF")]
    public ItemStatBonus defense = new ItemStatBonus();

    [FoldoutGroup("Stats/Resource Stats")] [LabelText("HP")]
    public ItemStatBonus health = new ItemStatBonus();

    [FoldoutGroup("Stats/Resource Stats")] [LabelText("Mana")]
    public ItemStatBonus mana = new ItemStatBonus();

    [FoldoutGroup("Stats/Special Stats")] [LabelText("Crit %")]
    public ItemStatBonus critChance = new ItemStatBonus();

    [FoldoutGroup("Stats/Special Stats")] [LabelText("Atk Speed")]
    public ItemStatBonus attackSpeed = new ItemStatBonus();

    [FoldoutGroup("Stats/Special Stats")] [LabelText("Life Steal")]
    public ItemStatBonus lifeSteal = new ItemStatBonus();

    [FoldoutGroup("Stats/Special Stats")] [LabelText("Move Speed")]
    public ItemStatBonus speed = new ItemStatBonus();

    [BoxGroup("Visuals")] [ColorPalette] public Color color;

    [BoxGroup("Visuals")] [PreviewField(60)]
    public Sprite iconLeft;

    [BoxGroup("Visuals")] [PreviewField(60)]
    public Sprite iconRight;

    private bool IsHorse => itemType == ItemType.Horse;

    [BoxGroup("Mount")] [ShowIf("@itemType == ItemType.Horse")]
    public HorseData horseData;

    [BoxGroup("Upgrade")] [LabelWidth(130)]
    public int baseUpgradeCost = 100;

    [Header("Consumable Settings")]
    public bool restoresHealth;
    public int healthRestoreAmount;

    public bool restoresMana;
    public int manaRestoreAmount;

    public bool percentageBased;

    // RANDOMIZER
    [BoxGroup("Randomizer")] public ItemTierConfig tierConfig;

    [BoxGroup("Randomizer"), LabelText("Locks")]
    public bool lockAttack;

    [BoxGroup("Randomizer")] public bool lockDefense;
    [BoxGroup("Randomizer")] public bool lockHealth;
    [BoxGroup("Randomizer")] public bool lockMana;
    [BoxGroup("Randomizer")] public bool lockCrit;
    [BoxGroup("Randomizer")] public bool lockAttackSpeed;
    [BoxGroup("Randomizer")] public bool lockLifeSteal;
    [BoxGroup("Randomizer")] public bool lockSpeed;

    public ItemDatabase itemDataBase;
    public GameObject prefab;

    private string BuildContextualDescription()
    {
        bool vi = LanguageManager.Instance != null && LanguageManager.Instance.CurrentLanguage == "vi";

        if (itemType == ItemType.Consumable)
            return BuildConsumableDescription(vi);

        string role = GetItemRoleText(vi);
        string focus = GetStatFocusText(vi);

        if (vi)
        {
            if (!string.IsNullOrEmpty(focus))
                return $"{role}. Phù hợp cho hành trình chiến đấu, giúp tăng {focus}.";

            return $"{role}. Một trang bị hữu ích cho nhà thám hiểm trong các trận chiến.";
        }

        if (!string.IsNullOrEmpty(_description) && !LooksLikePlaceholderDescription(_description))
            return _description;

        if (!string.IsNullOrEmpty(focus))
            return $"{role}. Useful in battle and improves {focus}.";

        return $"{role}. A useful piece of gear for adventurers in battle.";
    }

    private string BuildConsumableDescription(bool vi)
    {
        if (restoresHealth && restoresMana)
        {
            return vi
                ? $"Bình tiếp tế hồi {healthRestoreAmount} máu và {manaRestoreAmount} mana khi sử dụng."
                : $"A supply potion that restores {healthRestoreAmount} HP and {manaRestoreAmount} mana.";
        }

        if (restoresHealth)
        {
            return vi
                ? $"Bình hồi phục dùng trong chiến đấu, khôi phục {healthRestoreAmount} máu."
                : $"A combat potion that restores {healthRestoreAmount} HP.";
        }

        if (restoresMana)
        {
            return vi
                ? $"Bình năng lượng dùng trong chiến đấu, khôi phục {manaRestoreAmount} mana."
                : $"A mana potion that restores {manaRestoreAmount} mana.";
        }

        return vi
            ? "Vật phẩm tiêu hao có thể dùng trong hành trình."
            : "A consumable item for the journey.";
    }

    private string GetItemRoleText(bool vi)
    {
        return itemType switch
        {
            ItemType.Weapon => GetWeaponRoleText(vi),
            ItemType.Clother => vi ? "Áo giáp giúp giảm sát thương nhận vào" : "Armor that helps reduce incoming damage",
            ItemType.SpecialArmor => vi ? "Giáp đặc biệt dành cho những trận đánh khó" : "Special armor made for difficult battles",
            ItemType.Helmet => vi ? "Mũ bảo hộ giúp tăng khả năng sống sót" : "A helmet that improves survivability",
            ItemType.Boots => vi ? "Giày chiến đấu giúp di chuyển linh hoạt hơn" : "Combat boots that improve mobility",
            ItemType.Cloak => vi ? "Áo choàng hỗ trợ né tránh và áp sát" : "A cloak that supports evasion and positioning",
            ItemType.Horse => vi ? "Thú cưỡi hỗ trợ di chuyển trên bản đồ" : "A mount that helps with map movement",
            ItemType.Hair => vi ? "Trang phục ngoại hình giúp nhân vật nổi bật hơn" : "A cosmetic piece that changes the hero's look",
            _ => vi ? "Trang bị hỗ trợ nhân vật trong chiến đấu" : "Gear that supports the hero in battle"
        };
    }

    private string GetWeaponRoleText(bool vi)
    {
        return weaponCategory switch
        {
            WeaponCategory.Ranged => vi ? "Vũ khí tầm xa dùng để gây sát thương an toàn" : "A ranged weapon for dealing damage from safety",
            WeaponCategory.Bow => vi ? "Cung dùng để tấn công kẻ địch từ xa" : "A bow for striking enemies from afar",
            WeaponCategory.HeavyMelee => vi ? "Vũ khí hạng nặng gây sát thương mạnh ở cự ly gần" : "A heavy melee weapon that hits hard up close",
            _ => vi ? "Vũ khí cận chiến dùng để áp sát và hạ gục kẻ địch" : "A melee weapon for closing in and defeating enemies"
        };
    }

    private string GetStatFocusText(bool vi)
    {
        List<string> stats = new List<string>();

        AddStatFocus(stats, attack, vi ? "sát thương" : "damage");
        AddStatFocus(stats, defense, vi ? "phòng thủ" : "defense");
        AddStatFocus(stats, health, vi ? "máu tối đa" : "maximum HP");
        AddStatFocus(stats, mana, vi ? "mana tối đa" : "maximum mana");
        AddStatFocus(stats, critChance, vi ? "chí mạng" : "critical chance");
        AddStatFocus(stats, attackSpeed, vi ? "tốc đánh" : "attack speed");
        AddStatFocus(stats, lifeSteal, vi ? "hút máu" : "life steal");
        AddStatFocus(stats, speed, vi ? "tốc độ di chuyển" : "movement speed");

        if (stats.Count == 0)
            return "";

        return string.Join(vi ? ", " : ", ", stats.Take(3));
    }

    private void AddStatFocus(List<string> stats, ItemStatBonus bonus, string label)
    {
        if (bonus != null && bonus.HasValue)
            stats.Add(label);
    }

    private bool LooksLikePlaceholderDescription(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;

        string trimmed = value.Trim();
        return trimmed.EndsWith("_des", StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals("Lightweight and flexible", StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals("description", StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals("des", StringComparison.OrdinalIgnoreCase);
    }

    [BoxGroup("Randomizer")]
    [InfoBox("Nhấn để random lại cả flat và % theo tier. Bật lock để giữ stat cụ thể khi reroll.")]
    [Button("Randomize Stats")]
    public void RandomizeStats()
{
    if (tierConfig == null)
    {
        Debug.LogWarning($"[{itemName}] Missing TierConfig. Cannot randomize.");
        return;
    }

    var db = itemDataBase;
    var rule = db?.statRules.FirstOrDefault(r => r.itemType == itemType);

    if (rule == null)
    {
        Debug.LogWarning($"No stat rule for {itemType}, skipping...");
        return;
    }

    var range = tierConfig.GetRange(tier);

    float RoundPercent(float v) => Mathf.Round(v * 10f) / 10f;
    float RoundFlat(float v) => Mathf.Round(v);

    float RoundFlatFloat(float v) => Mathf.Round(v * 10f) / 10f;

    price = UnityEngine.Random.Range(range.priceRange.x, range.priceRange.y + 1);
    baseUpgradeCost = UnityEngine.Random.Range(range.upgradeCostRange.x, range.upgradeCostRange.y + 1);

    // ATK
    if (!lockAttack && rule.allowAttack)
    {
        attack = new ItemStatBonus(
            RoundFlat(UnityEngine.Random.Range(range.atkFlatRange.x, range.atkFlatRange.y)),
            RoundPercent(UnityEngine.Random.Range(range.atkPercentRange.x, range.atkPercentRange.y))
        );
    }
    else attack = ItemStatBonus.Zero;

    // DEF
    if (!lockDefense && rule.allowDefense)
    {
        defense = new ItemStatBonus(
            RoundFlat(UnityEngine.Random.Range(range.defFlatRange.x, range.defFlatRange.y)),
            RoundPercent(UnityEngine.Random.Range(range.defPercentRange.x, range.defPercentRange.y))
        );
    }
    else defense = ItemStatBonus.Zero;

    // HP
    if (!lockHealth && rule.allowHealth)
    {
        health = new ItemStatBonus(
            RoundFlat(UnityEngine.Random.Range(range.healthFlatRange.x, range.healthFlatRange.y)),
            RoundPercent(UnityEngine.Random.Range(range.healthPercentRange.x, range.healthPercentRange.y))
        );
    }
    else health = ItemStatBonus.Zero;

    // Mana
    if (!lockMana && rule.allowMana)
    {
        mana = new ItemStatBonus(
            RoundFlat(UnityEngine.Random.Range(range.manaFlatRange.x, range.manaFlatRange.y)),
            RoundPercent(UnityEngine.Random.Range(range.manaPercentRange.x, range.manaPercentRange.y))
        );
    }
    else mana = ItemStatBonus.Zero;

    // Crit
    if (!lockCrit && rule.allowCrit)
    {
        critChance = new ItemStatBonus(
            RoundFlat(UnityEngine.Random.Range(range.critFlatRange.x, range.critFlatRange.y)),
            RoundPercent(UnityEngine.Random.Range(range.critPercentRange.x, range.critPercentRange.y))
        );
    }
    else critChance = ItemStatBonus.Zero;

    // Attack Speed
    if (!lockAttackSpeed && rule.allowAttackSpeed)
    {
        attackSpeed = new ItemStatBonus(
            RoundFlatFloat(UnityEngine.Random.Range(range.attackSpeedFlatRange.x, range.attackSpeedFlatRange.y)),
            RoundPercent(UnityEngine.Random.Range(range.attackSpeedPercentRange.x, range.attackSpeedPercentRange.y))
        );
    }
    else attackSpeed = ItemStatBonus.Zero;

    // Life Steal
    if (!lockLifeSteal && rule.allowLifeSteal)
    {
        lifeSteal = new ItemStatBonus(
            RoundFlat(UnityEngine.Random.Range(range.lifeStealFlatRange.x, range.lifeStealFlatRange.y)),
            RoundPercent(UnityEngine.Random.Range(range.lifeStealPercentRange.x, range.lifeStealPercentRange.y))
        );
    }
    else lifeSteal = ItemStatBonus.Zero;

    // Move Speed
    if (!lockSpeed && rule.allowSpeed)
    {
        speed = new ItemStatBonus(
            RoundFlatFloat(UnityEngine.Random.Range(range.speedFlatRange.x, range.speedFlatRange.y)),
            RoundPercent(UnityEngine.Random.Range(range.speedPercentRange.x, range.speedPercentRange.y))
        );
    }
    else speed = ItemStatBonus.Zero;

#if UNITY_EDITOR
    UnityEditor.EditorUtility.SetDirty(this);
#endif
}


}
