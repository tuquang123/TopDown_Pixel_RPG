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

    // ============================
    // 1) PLAYER STAT
    // ============================
    public void SetStats(PlayerStats stats)
    {
        if (stats == null) return;

        attackText.text =
            $"<sprite name=\"attack_icon\" color=#FF4D4D> " +
            $"<color=#FFD700>Attack:</color> {stats.attack.Value}";

        defenseText.text =
            $"<sprite name=\"defense_icon\" color=#4DA6FF> " +
            $"<color=#FFD700>Defense:</color> {stats.defense.Value}";

        speedText.text =
            $"<sprite name=\"speed_icon\" color=#7CFF4D> " +
            $"<color=#FFD700>Speed:</color> {stats.speed.Value}";

        critText.text =
            $"<sprite name=\"crit_icon\" color=#FFB84D> " +
            $"<color=#FFD700>Crit:</color> {stats.critChance.Value}%";

        lifestealText.text =
            $"<sprite name=\"lifesteal_icon\" color=#C44DFF> " +
            $"<color=#FFD700>LifeSteal:</color> {stats.lifeSteal.Value}%";

        attackSpeedText.text =
            $"<sprite name=\"attackspeed_icon\" color=#4DFFE3> " +
            $"<color=#FFD700>AttSpeed:</color> {stats.GetAttackSpeed():0.0}";
    }

    // ============================
    // 2) ITEM STAT
    // ============================
    public void SetStats(ItemInstance item)
    {
        if (item == null || item.itemData == null) return;

        var data = item.itemData;

        SetText(attackText,      data.attack,      "attack_icon",      "#FF4D4D",  "Attack");
        SetText(defenseText,     data.defense,     "defense_icon",     "#4DA6FF",  "Defense");
        SetText(speedText,       data.speed,       "speed_icon",       "#7CFF4D",  "Speed");
        SetText(critText,        data.critChance,  "crit_icon",        "#FFB84D",  "Crit", "%");
        SetText(lifestealText,   data.lifeSteal,   "lifesteal_icon",   "#C44DFF",  "LifeSteal", "%");
        SetText(attackSpeedText, data.attackSpeed, "attackspeed_icon", "#4DFFE3",  "Atk Speed");
    }

    // ============================
    // Helper format cho item
    // ============================
    private void SetText(TextMeshProUGUI text, ItemStatBonus bonus, string icon, string color, string label, string suffix = "")
    {
        if (bonus == null || !bonus.HasValue)
        {
            text.text = "";
            return;
        }

        string flatPart = Mathf.Abs(bonus.flat) > 0.01f ? $"{bonus.flat}" : "";
        string percentPart = Mathf.Abs(bonus.percent) > 0.01f ? $"{bonus.percent}%" : "";
        string value = flatPart != "" ? flatPart : percentPart;

        text.text = $"<sprite name=\"{icon}\" color={color}> <color=#FFD700>{label}:</color> {value}{suffix}";
    }
}
