using UnityEngine;
using TMPro;

public class PlayerStatsPopupUI : BasePopup
{
    public StatDisplayComponent statDisplayComponent;

    [SerializeField] private PlayerStatsDataSO dataAsset;

    [Header("Cost x1")]
    [SerializeField] private TextMeshProUGUI attackCostText,      defenseCostText,      speedCostText,      critCostText,
                                             lifestealCostText,   attackSpeedCostText,  healthCostText,     manaCostText;

    [Header("Cost x5")]
    [SerializeField] private TextMeshProUGUI attackCostTextX5,    defenseCostTextX5,    speedCostTextX5,    critCostTextX5,
                                             lifestealCostTextX5, attackSpeedCostTextX5, healthCostTextX5,  manaCostTextX5;

    [Header("Cost x10")]
    [SerializeField] private TextMeshProUGUI attackCostTextX10,    defenseCostTextX10,    speedCostTextX10,    critCostTextX10,
                                             lifestealCostTextX10, attackSpeedCostTextX10, healthCostTextX10,  manaCostTextX10;

    private PlayerStatsDataContainer data => dataAsset.stats;

    public override void Show()
    {
        base.Show();
        dataAsset.Load();
        ApplyBaseStatsOnly();
        RefreshUI();
    }

    // ================= REFRESH =================

    private void RefreshUI()
    {
        statDisplayComponent.SetStats(PlayerStats.Instance);
        RefreshCostTexts();
    }

    private void RefreshCostTexts()
    {
        SetAll(1,
            attackCostText,    defenseCostText,    speedCostText,    critCostText,
            lifestealCostText, attackSpeedCostText, healthCostText,  manaCostText);

        SetAll(5,
            attackCostTextX5,    defenseCostTextX5,    speedCostTextX5,    critCostTextX5,
            lifestealCostTextX5, attackSpeedCostTextX5, healthCostTextX5,  manaCostTextX5);

        SetAll(10,
            attackCostTextX10,    defenseCostTextX10,    speedCostTextX10,    critCostTextX10,
            lifestealCostTextX10, attackSpeedCostTextX10, healthCostTextX10,  manaCostTextX10);
    }

    private void SetAll(int times,
        TextMeshProUGUI atk,  TextMeshProUGUI def, TextMeshProUGUI spd,  TextMeshProUGUI crit,
        TextMeshProUGUI ls,   TextMeshProUGUI aspd, TextMeshProUGUI hp,  TextMeshProUGUI mp)
    {
        SetCostText(atk,  data.attack,      times);
        SetCostText(def,  data.defense,     times);
        SetCostText(spd,  data.speed,       times);
        SetCostText(crit, data.crit,        times);
        SetCostText(ls,   data.lifesteal,   times);
        SetCostText(aspd, data.attackSpeed, times);
        SetCostText(hp,   data.health,      times);
        SetCostText(mp,   data.mana,        times);
    }

    private void SetCostText(TextMeshProUGUI text, PlayerStatData stat, int times)
    {
        if (text == null) return;

        long cost      = CalculateTotalCost(stat, times);
        int  safeCost  = cost > int.MaxValue ? int.MaxValue : (int)cost;
        bool canAfford = CurrencyManager.Instance.Gold >= safeCost;

        text.text = canAfford
            ? $"<color=#FFFFFF>{safeCost}</color> <sprite name=\"gold_icon\">"
            : $"<color=#FF4444>{safeCost}</color> <sprite name=\"gold_icon\">";
    }

    private long CalculateTotalCost(PlayerStatData stat, int times)
    {
        long total = 0;
        int  level = stat.level;

        for (int i = 0; i < times; i++)
        {
            total += stat.GetUpgradeCost(level);
            level++;
        }

        return total;
    }

    // ================= APPLY =================

    private void ApplyBaseStatsOnly()
    {
        var ps = PlayerStats.Instance;
        if (ps == null) return;

        ApplyIfHigher(ps.attack,      data.attack.GetValue());
        ApplyIfHigher(ps.defense,     data.defense.GetValue());
        ApplyIfHigher(ps.speed,       data.speed.GetValue());
        ApplyIfHigher(ps.critChance,  data.crit.GetValue());
        ApplyIfHigher(ps.lifeSteal,   data.lifesteal.GetValue());
        ApplyIfHigher(ps.attackSpeed, data.attackSpeed.GetValue());

        float newMaxHp = data.health.GetValue();
        if (newMaxHp > ps.maxHealth.baseValue)
            ps.maxHealth.SetBaseValue(newMaxHp);
        ps.currentHealth = Mathf.Clamp(ps.currentHealth, 1, (int)ps.maxHealth.Value);
        ps.NotifyHealthChanged();

        float newMaxMana = data.mana.GetValue();
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

    // ================= UPGRADE =================

    private void TryUpgrade(PlayerStatData stat, System.Func<PlayerStats, Stat> getter, int times)
    {
        long totalCost = 0;
        int  tempLevel = stat.level;

        for (int i = 0; i < times; i++)
        {
            totalCost += stat.GetUpgradeCost(tempLevel);
            tempLevel++;
        }

        int safeCost = totalCost > int.MaxValue ? int.MaxValue : (int)totalCost;

        if (!CurrencyManager.Instance.SpendGold(safeCost))
            return;

        stat.level = tempLevel;

        var ps         = PlayerStats.Instance;
        var playerStat = getter(ps);

        float oldMax = playerStat.Value;
        playerStat.SetBaseValue(stat.GetValue());
        float newMax = playerStat.Value;

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

    // ================= BUTTONS =================

    public void UpgradeAttackX1()       => TryUpgrade(data.attack,      ps => ps.attack,      1);
    public void UpgradeAttackX5()       => TryUpgrade(data.attack,      ps => ps.attack,      5);
    public void UpgradeAttackX10()      => TryUpgrade(data.attack,      ps => ps.attack,      10);

    public void UpgradeDefenseX1()      => TryUpgrade(data.defense,     ps => ps.defense,     1);
    public void UpgradeDefenseX5()      => TryUpgrade(data.defense,     ps => ps.defense,     5);
    public void UpgradeDefenseX10()     => TryUpgrade(data.defense,     ps => ps.defense,     10);

    public void UpgradeSpeedX1()        => TryUpgrade(data.speed,       ps => ps.speed,       1);
    public void UpgradeSpeedX5()        => TryUpgrade(data.speed,       ps => ps.speed,       5);
    public void UpgradeSpeedX10()       => TryUpgrade(data.speed,       ps => ps.speed,       10);

    public void UpgradeCritX1()         => TryUpgrade(data.crit,        ps => ps.critChance,  1);
    public void UpgradeCritX5()         => TryUpgrade(data.crit,        ps => ps.critChance,  5);
    public void UpgradeCritX10()        => TryUpgrade(data.crit,        ps => ps.critChance,  10);

    public void UpgradeLifestealX1()    => TryUpgrade(data.lifesteal,   ps => ps.lifeSteal,   1);
    public void UpgradeLifestealX5()    => TryUpgrade(data.lifesteal,   ps => ps.lifeSteal,   5);
    public void UpgradeLifestealX10()   => TryUpgrade(data.lifesteal,   ps => ps.lifeSteal,   10);

    public void UpgradeAttackSpeedX1()  => TryUpgrade(data.attackSpeed, ps => ps.attackSpeed, 1);
    public void UpgradeAttackSpeedX5()  => TryUpgrade(data.attackSpeed, ps => ps.attackSpeed, 5);
    public void UpgradeAttackSpeedX10() => TryUpgrade(data.attackSpeed, ps => ps.attackSpeed, 10);

    public void UpgradeHealthX1()       => TryUpgrade(data.health,      ps => ps.maxHealth,   1);
    public void UpgradeHealthX5()       => TryUpgrade(data.health,      ps => ps.maxHealth,   5);
    public void UpgradeHealthX10()      => TryUpgrade(data.health,      ps => ps.maxHealth,   10);

    public void UpgradeManaX1()         => TryUpgrade(data.mana,        ps => ps.maxMana,     1);
    public void UpgradeManaX5()         => TryUpgrade(data.mana,        ps => ps.maxMana,     5);
    public void UpgradeManaX10()        => TryUpgrade(data.mana,        ps => ps.maxMana,     10);

    public void Close() => UIManager.Instance.HidePopupByType(PopupType.Stats);
}