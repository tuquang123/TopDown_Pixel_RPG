using TMPro;
using UnityEngine;

public class StatDisplayComponent : MonoBehaviour
{
    [Header("Stat Texts")]
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI critText;
    [SerializeField] private TextMeshProUGUI lifestealText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private TextMeshProUGUI healText;
    [SerializeField] private TextMeshProUGUI manaText;
   
    // =========================================================
// 3) ITEM COMPARE (preview vs equipped)
// =========================================================
    public void SetCompareStats(ItemInstance preview, ItemInstance equipped)
    {
        if (preview == null || preview.itemData == null) return;

        int previewLv = Mathf.Max(1, preview.upgradeLevel);
        int equippedLv = equipped != null ? Mathf.Max(1, equipped.upgradeLevel) : 0;

        var p = preview.itemData;
        var e = equipped != null ? equipped.itemData : null;

        SetCompareStat(attackText,      p.attack,      e?.attack,      previewLv, equippedLv, "attack_icon",      "#FF8C00", "Attack");
        SetCompareStat(defenseText,     p.defense,     e?.defense,     previewLv, equippedLv, "defense_icon",     "#808080", "Defense");
        SetCompareStat(speedText,       p.speed,       e?.speed,       previewLv, equippedLv, "speed_icon",       "#7CFF4D", "Speed");
        SetCompareStat(critText,        p.critChance,  e?.critChance,  previewLv, equippedLv, "crit_icon",        "#FFB84D", "Crit", true);
        SetCompareStat(lifestealText,   p.lifeSteal,   e?.lifeSteal,   previewLv, equippedLv, "lifesteal_icon",   "#C44DFF", "LifeSteal", true);
        SetCompareStat(attackSpeedText, p.attackSpeed, e?.attackSpeed, previewLv, equippedLv, "attackspeed_icon", "#4DFFE3", "Atk Speed");
        SetCompareStat(healText,        p.health,      e?.health,      previewLv, equippedLv, "health_icon",      "#FF3333", "HP");
        SetCompareStat(manaText,        p.mana,        e?.mana,        previewLv, equippedLv, "mana_icon",        "#3399FF", "Mana");
    }
    private void SetCompareStat(
        TextMeshProUGUI text,
        ItemStatBonus previewBonus,
        ItemStatBonus equippedBonus,
        int previewLv,
        int equippedLv,
        string icon,
        string color,
        string label,
        bool isPercent = false
    )
    {
        if (previewBonus == null || !previewBonus.HasValue)
        {
            text.gameObject.SetActive(false);
            return;
        }

        text.gameObject.SetActive(true);

        float previewValue = GetValue(previewBonus, previewLv);
        float equippedValue = equippedBonus != null
            ? GetValue(equippedBonus, equippedLv)
            : 0;

        float diff = previewValue - equippedValue;

        string diffText = "";
        if (Mathf.Abs(diff) > 0.01f)
        {
            string sign = diff > 0 ? "+" : "";
            string diffColor = diff > 0 ? "#00FF00" : "#FF4444";
            diffText =
                $" <color={diffColor}>({sign}{Format(diff)}{(isPercent ? "%" : "")})</color>";
        }

        text.text =
            $"<sprite name=\"{icon}\" color={color}> " +
            $"<color=#FFD700>{label}:</color> {Format(previewValue)}{(isPercent ? "%" : "")}" +
            diffText;
    }
    private float GetValue(ItemStatBonus bonus, int level)
    {
        if (bonus == null || !bonus.HasValue) return 0;

        if (Mathf.Abs(bonus.flat) > 0.01f)
            return Equipment.ItemStatCalculator.GetUpgradedValue(bonus.flat, level);

        return Equipment.ItemStatCalculator.GetUpgradedValue(bonus.percent, level);
    }


    // =========================================================
    // 1) PLAYER STATS (đang mặc – tính từ PlayerStats)
    // =========================================================
    public void SetStats(PlayerStats stats)
    {
        if (stats == null) return;
      

        SetPlayerStat(attackText,      "attack_icon",      "#FF8C00", "Attack",      stats.attack);
        SetPlayerStat(defenseText,     "defense_icon",     "#808080", "Defense",     stats.defense);
        SetPlayerStat(speedText,       "speed_icon",       "#7CFF4D", "Speed",       stats.speed);
        SetPlayerStat(critText,        "crit_icon",        "#FFB84D", "Crit",        stats.critChance, "%");
        SetPlayerStat(lifestealText,   "lifesteal_icon",   "#C44DFF", "LifeSteal",   stats.lifeSteal, "%");
        SetPlayerStat(attackSpeedText, "attackspeed_icon", "#4DFFE3", "Atk Speed",   stats.attackSpeed);
        SetPlayerStat(healText,        "health_icon",      "#FF3333", "HP",          stats.maxHealth);
        SetPlayerStat(manaText,        "mana_icon",        "#3399FF", "Mana",        stats.maxMana);
    }

    private void SetPlayerStat(
        TextMeshProUGUI text,
        string icon,
        string color,
        string label,
        Stat stat,
        string suffix = ""
    )
    {
        float baseValue = stat.baseValue;
        float bonus = stat.Value - baseValue;

        if (bonus > 0.01f)
        {
            text.text =
                $"<sprite name=\"{icon}\" color={color}> " +
                $"<color=#FFD700>{label}:</color> {Format(baseValue)}{suffix} " +
                $"<color=#00FF00>(+{Format(bonus)}{suffix})</color>";
        }
        else
        {
            text.text =
                $"<sprite name=\"{icon}\" color={color}> " +
                $"<color=#FFD700>{label}:</color> {Format(baseValue)}{suffix}";
        }
    }

    // =========================================================
    // 2) ITEM STATS (DETAIL – preview theo upgradeLevel)
    // =========================================================
    public void SetStats(ItemInstance item)
    {
        if (item == null || item.itemData == null) return;

        int level = Mathf.Max(1, item.upgradeLevel);
        var data = item.itemData;

        SetItemStat(attackText,      data.attack,      level, "attack_icon",      "#FF8C00", "Attack");
        SetItemStat(defenseText,     data.defense,     level, "defense_icon",     "#808080", "Defense");
        SetItemStat(speedText,       data.speed,       level, "speed_icon",       "#7CFF4D", "Speed");
        SetItemStat(critText,        data.critChance,  level, "crit_icon",        "#FFB84D", "Crit", "%");
        SetItemStat(lifestealText,   data.lifeSteal,   level, "lifesteal_icon",   "#C44DFF", "LifeSteal", "%");
        SetItemStat(attackSpeedText, data.attackSpeed, level, "attackspeed_icon", "#4DFFE3", "Atk Speed");
        SetItemStat(healText,        data.health,      level, "health_icon",      "#FF3333", "HP");
        SetItemStat(manaText,        data.mana,        level, "mana_icon",        "#3399FF", "Mana");
    }

    private void SetItemStat(
        TextMeshProUGUI text,
        ItemStatBonus bonus,
        int level,
        string icon,
        string color,
        string label,
        string suffix = ""
    )
    {
        if (bonus == null || !bonus.HasValue)
        {
            text.gameObject.SetActive(false);
            return;
        }

        text.gameObject.SetActive(true);

        string value = "";

        if (Mathf.Abs(bonus.flat) > 0.01f)
        {
            float v = Equipment.ItemStatCalculator.GetUpgradedValue(
                bonus.flat,
                level
            );
            value = Format(v);
        }
        else if (Mathf.Abs(bonus.percent) > 0.01f)
        {
            float v = Equipment.ItemStatCalculator.GetUpgradedValue(
                bonus.percent,
                level
            );
            value = $"{Format(v)}%";
        }

        text.text =
            $"<sprite name=\"{icon}\" color={color}> " +
            $"<color=#FFD700>{label}:</color> {value}{suffix}";
    }

    // =========================================================
    // UTILS
    // =========================================================
    private string Format(float value)
    {
        return value % 1 == 0
            ? value.ToString("0")
            : value.ToString("0.0");
    }
}
