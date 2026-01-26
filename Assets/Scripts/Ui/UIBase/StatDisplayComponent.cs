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

    // ============================
    // 1) PLAYER STAT
    // ============================
 public void SetStats(PlayerStats stats)
{
    if (stats == null) return;

    // ===== ATTACK =====
    float baseAtk = stats.attack.baseValue;
    float bonusAtk = stats.attack.Value - baseAtk;
    attackText.text =
        bonusAtk > 0.01f
            ? $"<sprite name=\"attack_icon\" color=#FF8C00> <color=#FFD700>Attack:</color> {FormatStat(baseAtk)} <color=#00FF00>(+{FormatStat(bonusAtk)})</color>"
            : $"<sprite name=\"attack_icon\" color=#FF8C00> <color=#FFD700>Attack:</color> {FormatStat(baseAtk)}";

    // ===== DEFENSE =====
    float baseDef = stats.defense.baseValue;
    float bonusDef = stats.defense.Value - baseDef;
    defenseText.text =
        bonusDef > 0.01f
            ? $"<sprite name=\"defense_icon\" color=#808080> <color=#FFD700>Defense:</color> {FormatStat(baseDef)} <color=#00FF00>(+{FormatStat(bonusDef)})</color>"
            : $"<sprite name=\"defense_icon\" color=#808080> <color=#FFD700>Defense:</color> {FormatStat(baseDef)}";

    // ===== SPEED =====
    float baseSpd = stats.speed.baseValue;
    float bonusSpd = stats.speed.Value - baseSpd;
    speedText.text =
        bonusSpd > 0.01f
            ? $"<sprite name=\"speed_icon\" color=#7CFF4D> <color=#FFD700>Speed:</color> {FormatStat(baseSpd)} <color=#00FF00>(+{FormatStat(bonusSpd)})</color>"
            : $"<sprite name=\"speed_icon\" color=#7CFF4D> <color=#FFD700>Speed:</color> {FormatStat(baseSpd)}";

    // ===== CRIT =====
    float baseCrit = stats.critChance.baseValue;
    float bonusCrit = stats.critChance.Value - baseCrit;
    critText.text =
        bonusCrit > 0.01f
            ? $"<sprite name=\"crit_icon\" color=#FFB84D> <color=#FFD700>Crit:</color> {FormatStat(baseCrit)}% <color=#00FF00>(+{FormatStat(bonusCrit)}%)</color>"
            : $"<sprite name=\"crit_icon\" color=#FFB84D> <color=#FFD700>Crit:</color> {FormatStat(baseCrit)}%";

    // ===== LIFESTEAL =====
    float baseLS = stats.lifeSteal.baseValue;
    float bonusLS = stats.lifeSteal.Value - baseLS;
    lifestealText.text =
        bonusLS > 0.01f
            ? $"<sprite name=\"lifesteal_icon\" color=#C44DFF> <color=#FFD700>LifeSteal:</color> {FormatStat(baseLS)}% <color=#00FF00>(+{FormatStat(bonusLS)}%)</color>"
            : $"<sprite name=\"lifesteal_icon\" color=#C44DFF> <color=#FFD700>LifeSteal:</color> {FormatStat(baseLS)}%";

    // ===== ATTACK SPEED =====
    float baseAtkSpd = stats.attackSpeed.baseValue;
    float bonusAtkSpd = stats.attackSpeed.Value - baseAtkSpd;
    attackSpeedText.text =
        bonusAtkSpd > 0.01f
            ? $"<sprite name=\"attackspeed_icon\" color=#4DFFE3> <color=#FFD700>AttSpeed:</color> {FormatStat(baseAtkSpd)} <color=#00FF00>(+{FormatStat(bonusAtkSpd)})</color>"
            : $"<sprite name=\"attackspeed_icon\" color=#4DFFE3> <color=#FFD700>AttSpeed:</color> {FormatStat(baseAtkSpd)}";

    // ===== HP =====
    float baseHP = stats.maxHealth.baseValue;
    float bonusHP = stats.maxHealth.Value - baseHP;
    healText.text =
        bonusHP > 0.01f
            ? $"<sprite name=\"health_icon\" color=#FF3333> <color=#FFD700>HP:</color> {FormatStat(baseHP)} <color=#00FF00>(+{FormatStat(bonusHP)})</color>"
            : $"<sprite name=\"health_icon\" color=#FF3333> <color=#FFD700>HP:</color> {FormatStat(baseHP)}";

    // ===== MANA =====
    float baseMana = stats.maxMana.baseValue;
    float bonusMana = stats.maxMana.Value - baseMana;
    manaText.text =
        bonusMana > 0.01f
            ? $"<sprite name=\"mana_icon\" color=#3399FF> <color=#FFD700>Mana:</color> {FormatStat(baseMana)} <color=#00FF00>(+{FormatStat(bonusMana)})</color>"
            : $"<sprite name=\"mana_icon\" color=#3399FF> <color=#FFD700>Mana:</color> {FormatStat(baseMana)}";
}



    // ============================
    // 2) ITEM STAT
    // ============================
    public void SetStats(ItemInstance item)
    {
        if (item == null || item.itemData == null) return;

        var data = item.itemData;

        SetText(attackText,      data.attack,      "attack_icon",      "#FF8C00", "Attack");
        SetText(defenseText,     data.defense,     "defense_icon",     "#808080", "Defense");
        SetText(speedText,       data.speed,       "speed_icon",       "#7CFF4D", "Speed");
        SetText(critText,        data.critChance,  "crit_icon",        "#FFB84D", "Crit", "%");
        SetText(lifestealText,   data.lifeSteal,   "lifesteal_icon",   "#C44DFF", "LifeSteal", "%");
        SetText(attackSpeedText, data.attackSpeed, "attackspeed_icon", "#4DFFE3", "Atk Speed");
        SetText(healText,        data.health,      "health_icon",          "#FF3333", "HP");
        SetText(manaText,        data.mana,        "mana_icon",        "#3399FF", "Mana");
    }

    // Helper cho Item
    private void SetText(TextMeshProUGUI text, ItemStatBonus bonus, string icon, string color, string label, string suffix = "")
    {
        if (bonus == null || !bonus.HasValue)
        {
            text.gameObject.SetActive(false); // ⛔ Ẩn hoàn toàn dòng stat
            return;
        }

        text.gameObject.SetActive(true); // ✔ Hiện lại khi stat có giá trị

        string flatPart = Mathf.Abs(bonus.flat) > 0.01f ? $"{bonus.flat}" : "";
        string percentPart = Mathf.Abs(bonus.percent) > 0.01f ? $"{bonus.percent}%" : "";
        string value = flatPart != "" ? flatPart : percentPart;

        text.text =
            $"<sprite name=\"{icon}\" color={color}> " +
            $"<color=#FFD700>{label}:</color> {value}{suffix}";
    }
    private string FormatStat(float value)
    {
        return value % 1 == 0
            ? value.ToString("0")
            : value.ToString("0.0");
    }

}
