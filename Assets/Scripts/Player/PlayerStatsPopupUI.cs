using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatsPopupUI : BasePopup
{
    public StatDisplayComponent statDisplayComponent;

    [SerializeField] private PlayerStatsDataSO dataAsset;

    [Header("Cost Texts")]
    [SerializeField] private TextMeshProUGUI attackCostText, defenseCostText, speedCostText, critCostText,
                                             lifestealCostText, attackSpeedCostText, healthCostText, manaCostText;

    [Header("Preview Texts")]
    [SerializeField] private TextMeshProUGUI attackPreview, defensePreview, speedPreview, critPreview,
                                             lifestealPreview, attackSpeedPreview, healthPreview, manaPreview;

    [Header("Multiplier Buttons")]
    [SerializeField] private Button btnX1, btnX10, btnX100;

    private PlayerStatsDataContainer data => dataAsset.stats;
    private int currentMultiplier = 1;

    public override void Show()
    {
        base.Show();
        dataAsset.Load();
        ApplyBaseStatsOnly();
        SetMultiplier(1);
    }

    private void SetMultiplier(int value)
    {
        currentMultiplier = value;

        btnX1.interactable   = value != 1;
        btnX10.interactable  = value != 10;
        btnX100.interactable = value != 100;

        RefreshUI();
    }

    private void RefreshUI()
    {
        statDisplayComponent.SetStats(PlayerStats.Instance);
        RefreshCostTexts();
        RefreshPreviewTexts();
    }

    private void RefreshCostTexts()
    {
        SetCostText(attackCostText,      data.attack,      currentMultiplier);
        SetCostText(defenseCostText,     data.defense,     currentMultiplier);
        SetCostText(speedCostText,       data.speed,       currentMultiplier);
        SetCostText(critCostText,        data.crit,        currentMultiplier);
        SetCostText(lifestealCostText,   data.lifesteal,   currentMultiplier);
        SetCostText(attackSpeedCostText, data.attackSpeed, currentMultiplier);
        SetCostText(healthCostText,      data.health,      currentMultiplier);
        SetCostText(manaCostText,        data.mana,        currentMultiplier);
    }

    private void RefreshPreviewTexts()
    {
        var ps = PlayerStats.Instance;
        if (ps == null) return;

        SetPreviewText(attackPreview,      data.attack,      ps.attack.Value,      currentMultiplier, false);
        SetPreviewText(defensePreview,     data.defense,     ps.defense.Value,     currentMultiplier, false);
        SetPreviewText(speedPreview,       data.speed,       ps.speed.Value,       currentMultiplier, false);
        SetPreviewText(critPreview,        data.crit,        ps.critChance.Value,  currentMultiplier, true);
        SetPreviewText(lifestealPreview,   data.lifesteal,   ps.lifeSteal.Value,   currentMultiplier, true);
        SetPreviewText(attackSpeedPreview, data.attackSpeed, ps.attackSpeed.Value, currentMultiplier, true);
        SetPreviewText(healthPreview,      data.health,      ps.maxHealth.Value,   currentMultiplier, false);
        SetPreviewText(manaPreview,        data.mana,        ps.maxMana.Value,     currentMultiplier, false);
    }

    private void SetPreviewText(TextMeshProUGUI text, PlayerStatData stat, float currentValue, int times, bool isPercent)
    {
        if (text == null) return;

        float nextValue = currentValue + times * stat.increasePerLevel;

        string current = FormatStatValue(currentValue, isPercent);
        string next    = FormatStatValue(nextValue,    isPercent);

        text.text = $"{current} <color=#888888>>></color> <color=#00FF99>{next}</color>";
    }
    private string FormatStatValue(float value, bool isPercent)
    {
        if (isPercent)
            return $"{value:0.#}%";

        return $"{value:0.#}";
    }

    private string FormatGoldValue(long value)
    {
        if (value >= 1000000)
            return $"{value / 1000000f:0.#}M";

        if (value >= 1000)
            return $"{value / 1000f:0.#}K";

        return value.ToString();
    }

    private void SetCostText(TextMeshProUGUI text, PlayerStatData stat, int times)
    {
        if (text == null) return;

        long cost      = CalculateTotalCost(stat, times);
        int  safeCost  = cost > int.MaxValue ? int.MaxValue : (int)cost;
        bool canAfford = CurrencyManager.Instance.Gold >= safeCost;
        string display = CurrencyManager.FormatGold(safeCost);

        text.text = canAfford
            ? $"<color=#FFFFFF>{display}</color> <sprite name=\"gold_icon\">"
            : $"<color=#FF4444>{display}</color> <sprite name=\"gold_icon\">";
    }
    
    private void ApplyBaseStatsOnly()
    {
        var ps = PlayerStats.Instance;
        if (ps == null) return;

        ApplyIfHigher(ps.attack,      data.attack.GetValueAtLevel(dataAsset.GetSavedLevel(data.attack)));
        ApplyIfHigher(ps.defense,     data.defense.GetValueAtLevel(dataAsset.GetSavedLevel(data.defense)));
        ApplyIfHigher(ps.speed,       data.speed.GetValueAtLevel(dataAsset.GetSavedLevel(data.speed)));
        ApplyIfHigher(ps.critChance,  data.crit.GetValueAtLevel(dataAsset.GetSavedLevel(data.crit)));
        ApplyIfHigher(ps.lifeSteal,   data.lifesteal.GetValueAtLevel(dataAsset.GetSavedLevel(data.lifesteal)));
        ApplyIfHigher(ps.attackSpeed, data.attackSpeed.GetValueAtLevel(dataAsset.GetSavedLevel(data.attackSpeed)));

        float newMaxHp = data.health.GetValueAtLevel(dataAsset.GetSavedLevel(data.health));
        if (newMaxHp > ps.maxHealth.baseValue)
            ps.maxHealth.SetBaseValue(newMaxHp);
        ps.currentHealth = Mathf.Clamp(ps.currentHealth, 1, (int)ps.maxHealth.Value);
        ps.NotifyHealthChanged();

        float newMaxMana = data.mana.GetValueAtLevel(dataAsset.GetSavedLevel(data.mana));
        if (newMaxMana > ps.maxMana.baseValue)
            ps.maxMana.SetBaseValue(newMaxMana);
        ps.currentMana = Mathf.Clamp(ps.currentMana, 0, (int)ps.maxMana.Value);
        ps.NotifyManaChanged();

        ps.CalculatePower();
        ps.NotifyStatsChanged();
    }

    private void ApplyIfHigher(Stat stat, float value)
    {
        if (value > stat.baseValue)
            stat.SetBaseValue(value);
    }

    private long CalculateTotalCost(PlayerStatData stat, int times)
    {
        var ps = PlayerStats.Instance;
        if (ps == null) return 0;

        // Cost nâng cấp phải bám theo base stat đã nâng cấp (không tính buff tạm thời từ modifier).
        float runtimeValue = GetUpgradeableRuntimeValueForStat(ps, stat);
        int currentLevel = stat.GetLevelFromValue(runtimeValue);

        long total = 0;
        for (int i = 0; i < times; i++)
        {
            total += stat.GetUpgradeCost(currentLevel + i);
        }

        return total;
    }

    private float GetUpgradeableRuntimeValueForStat(PlayerStats ps, PlayerStatData stat)
    {
        // Dùng baseValue để level/cost không bị lệch bởi buff/debuff runtime.
        if (stat == data.attack) return ps.attack.baseValue;
        if (stat == data.defense) return ps.defense.baseValue;
        if (stat == data.speed) return ps.speed.baseValue;
        if (stat == data.crit) return ps.critChance.baseValue;
        if (stat == data.lifesteal) return ps.lifeSteal.baseValue;
        if (stat == data.attackSpeed) return ps.attackSpeed.baseValue;
        if (stat == data.health) return ps.maxHealth.baseValue;
        if (stat == data.mana) return ps.maxMana.baseValue;

        return stat.GetValueAtLevel(dataAsset.GetSavedLevel(stat));
    }

    private void TryUpgrade(PlayerStatData stat, System.Func<PlayerStats, Stat> getter, int times)
    {
        long totalCost = CalculateTotalCost(stat, times);
        int  safeCost  = totalCost > int.MaxValue ? int.MaxValue : (int)totalCost;

        if (!CurrencyManager.Instance.SpendGold(safeCost))
            return;

        var ps         = PlayerStats.Instance;
        var playerStat = getter(ps);
        int currentLevel = stat.GetLevelFromValue(playerStat.baseValue);
        int nextLevel = currentLevel + times;

        float oldMax = playerStat.Value;
        playerStat.SetBaseValue(stat.GetValueAtLevel(nextLevel));
        float newMax = playerStat.Value;
        dataAsset.SetSavedLevel(stat, nextLevel);

        if (playerStat == ps.maxHealth && oldMax > 0)
        {
            ps.currentHealth = (int)newMax;
            ps.NotifyHealthChanged();
        }
        else if (playerStat == ps.maxMana && oldMax > 0)
        {
            ps.currentMana = (int)newMax;
            ps.NotifyManaChanged();
        }

        ps.CalculatePower();
        ps.NotifyStatsChanged();

        dataAsset.Save();
        RefreshUI();
    }

    public void OnClickX1()   => SetMultiplier(1);
    public void OnClickX10()  => SetMultiplier(10);
    public void OnClickX100() => SetMultiplier(100);

    public void UpgradeAttack()      => TryUpgrade(data.attack,      ps => ps.attack,      currentMultiplier);
    public void UpgradeDefense()     => TryUpgrade(data.defense,     ps => ps.defense,     currentMultiplier);
    public void UpgradeSpeed()       => TryUpgrade(data.speed,       ps => ps.speed,       currentMultiplier);
    public void UpgradeCrit()        => TryUpgrade(data.crit,        ps => ps.critChance,  currentMultiplier);
    public void UpgradeLifesteal()   => TryUpgrade(data.lifesteal,   ps => ps.lifeSteal,   currentMultiplier);
    public void UpgradeAttackSpeed() => TryUpgrade(data.attackSpeed, ps => ps.attackSpeed, currentMultiplier);
    public void UpgradeHealth()      => TryUpgrade(data.health,      ps => ps.maxHealth,   currentMultiplier);
    public void UpgradeMana()        => TryUpgrade(data.mana,        ps => ps.maxMana,     currentMultiplier);

    public void Close() => UIManager.Instance.HidePopupByType(PopupType.Stats);
}
