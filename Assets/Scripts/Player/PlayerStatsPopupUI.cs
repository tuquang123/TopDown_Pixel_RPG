using UnityEngine;
using TMPro;
using DG.Tweening;

public class PlayerStatsPopupUI : BasePopup
{
    public StatDisplayComponent statDisplayComponent;

    [SerializeField] private PlayerStatsDataSO dataAsset;

    [Header("Cost x1")]
    [SerializeField] private TextMeshProUGUI attackCostText, defenseCostText, speedCostText, critCostText,
        lifestealCostText, attackSpeedCostText, healthCostText, manaCostText;

    [Header("Cost x5")]
    [SerializeField] private TextMeshProUGUI attackCostTextX5, defenseCostTextX5, speedCostTextX5, critCostTextX5,
        lifestealCostTextX5, attackSpeedCostTextX5, healthCostTextX5, manaCostTextX5;

    [Header("Cost x10")]
    [SerializeField] private TextMeshProUGUI attackCostTextX10, defenseCostTextX10, speedCostTextX10, critCostTextX10,
        lifestealCostTextX10, attackSpeedCostTextX10, healthCostTextX10, manaCostTextX10;

    [Header("Rows")]
    [SerializeField] private RectTransform attackRow, defenseRow, speedRow, critRow,
        lifestealRow, attackSpeedRow, healthRow, manaRow;

    private PlayerStatsDataContainer data => dataAsset.stats;

    public override void Show()
    {
        base.Show();
        dataAsset.Load();
        ApplyAllDataToPlayerStats();
        RefreshUI();
    }

    private void RefreshUI()
    {
        statDisplayComponent.SetStats(PlayerStats.Instance);
        RefreshCostTexts();
    }

    // ================= COST =================

    private void RefreshCostTexts()
    {
        SetAll(1, attackCostText, defenseCostText, speedCostText, critCostText,
            lifestealCostText, attackSpeedCostText, healthCostText, manaCostText);

        SetAll(5, attackCostTextX5, defenseCostTextX5, speedCostTextX5, critCostTextX5,
            lifestealCostTextX5, attackSpeedCostTextX5, healthCostTextX5, manaCostTextX5);

        SetAll(10, attackCostTextX10, defenseCostTextX10, speedCostTextX10, critCostTextX10,
            lifestealCostTextX10, attackSpeedCostTextX10, healthCostTextX10, manaCostTextX10);
    }

    private void SetAll(int times,
        TextMeshProUGUI atk, TextMeshProUGUI def, TextMeshProUGUI spd, TextMeshProUGUI crit,
        TextMeshProUGUI ls, TextMeshProUGUI aspd, TextMeshProUGUI hp, TextMeshProUGUI mp)
    {
        SetCostText(atk, data.attack, times);
        SetCostText(def, data.defense, times);
        SetCostText(spd, data.speed, times);
        SetCostText(crit, data.crit, times);
        SetCostText(ls, data.lifesteal, times);
        SetCostText(aspd, data.attackSpeed, times);
        SetCostText(hp, data.health, times);
        SetCostText(mp, data.mana, times);
    }

    private void SetCostText(TextMeshProUGUI text, PlayerStatData stat, int times)
    {
        if (text == null) return;

        long cost = CalculateTotalCost(stat, times);

        int safeCost = cost > int.MaxValue ? int.MaxValue : (int)cost;
        bool canAfford = CurrencyManager.Instance.Gold >= safeCost;

        text.text = canAfford
            ? $"<color=#FFFFFF>{safeCost}</color> <sprite name=\"gold_icon\">"
            : $"<color=#FF4444>{safeCost}</color> <sprite name=\"gold_icon\">";
    }

    private long CalculateTotalCost(PlayerStatData stat, int times)
    {
        long total = 0;
        int level = stat.level;

        for (int i = 0; i < times; i++)
        {
            total += stat.GetUpgradeCost(level);
            level++;
        }

        return total;
    }

    // ================= APPLY =================

    private void ApplyAllDataToPlayerStats()
    {
        var ps = PlayerStats.Instance;

        ps.attack.SetBaseValue(data.attack.GetValue());
        ps.defense.SetBaseValue(data.defense.GetValue());
        ps.speed.SetBaseValue(data.speed.GetValue());
        ps.critChance.SetBaseValue(data.crit.GetValue());
        ps.lifeSteal.SetBaseValue(data.lifesteal.GetValue());
        ps.attackSpeed.SetBaseValue(data.attackSpeed.GetValue());
        ps.maxHealth.SetBaseValue(data.health.GetValue());
        ps.maxMana.SetBaseValue(data.mana.GetValue());
    }

    // ================= UPGRADE =================

    private void TryUpgrade(PlayerStatData stat, System.Func<PlayerStats, Stat> getter, RectTransform row, int times)
    {
        int actualUpgrades = 0;
        long totalCost = 0;
        int tempLevel = stat.level;

        // 🔥 tính đúng số lần có thể nâng
        for (int i = 0; i < times; i++)
        {
            if (tempLevel >= stat.maxLevel) break;

            totalCost += stat.GetUpgradeCost(tempLevel);
            tempLevel++;
            actualUpgrades++;
        }

        if (actualUpgrades == 0) return;

        int safeCost = totalCost > int.MaxValue ? int.MaxValue : (int)totalCost;

        if (!CurrencyManager.Instance.SpendGold(safeCost))
        {
            row?.DOShakePosition(0.3f, new Vector3(6f, 0, 0));
            return;
        }

        // 🔥 upgrade đúng số lần
        stat.level = tempLevel;

        // 🔥 update stat NGAY LẬP TỨC
        var playerStat = getter(PlayerStats.Instance);
        playerStat.SetBaseValue(stat.GetValue());

        dataAsset.Save();

        row?.DOPunchScale(Vector3.one * 0.1f, 0.3f);

        RefreshUI();
    }
    // ===== BUTTONS =====

    public void UpgradeAttackX1() => TryUpgrade(data.attack, ps => ps.attack, attackRow, 1);
    public void UpgradeAttackX5() => TryUpgrade(data.attack, ps => ps.attack, attackRow, 5);
    public void UpgradeAttackX10() => TryUpgrade(data.attack, ps => ps.attack, attackRow, 10);

    public void UpgradeDefenseX1() => TryUpgrade(data.defense, ps => ps.defense, defenseRow, 1);
    public void UpgradeDefenseX5() => TryUpgrade(data.defense, ps => ps.defense, defenseRow, 5);
    public void UpgradeDefenseX10() => TryUpgrade(data.defense, ps => ps.defense, defenseRow, 10);

    public void UpgradeSpeedX1() => TryUpgrade(data.speed, ps => ps.speed, speedRow, 1);
    public void UpgradeSpeedX5() => TryUpgrade(data.speed, ps => ps.speed, speedRow, 5);
    public void UpgradeSpeedX10() => TryUpgrade(data.speed, ps => ps.speed, speedRow, 10);

    public void UpgradeCritX1() => TryUpgrade(data.crit, ps => ps.critChance, critRow, 1);
    public void UpgradeCritX5() => TryUpgrade(data.crit, ps => ps.critChance, critRow, 5);
    public void UpgradeCritX10() => TryUpgrade(data.crit, ps => ps.critChance, critRow, 10);

    public void UpgradeLifestealX1() => TryUpgrade(data.lifesteal, ps => ps.lifeSteal, lifestealRow, 1);
    public void UpgradeLifestealX5() => TryUpgrade(data.lifesteal, ps => ps.lifeSteal, lifestealRow, 5);
    public void UpgradeLifestealX10() => TryUpgrade(data.lifesteal, ps => ps.lifeSteal, lifestealRow, 10);

    public void UpgradeAttackSpeedX1() => TryUpgrade(data.attackSpeed, ps => ps.attackSpeed, attackSpeedRow, 1);
    public void UpgradeAttackSpeedX5() => TryUpgrade(data.attackSpeed, ps => ps.attackSpeed, attackSpeedRow, 5);
    public void UpgradeAttackSpeedX10() => TryUpgrade(data.attackSpeed, ps => ps.attackSpeed, attackSpeedRow, 10);

    public void UpgradeHealthX1() => TryUpgrade(data.health, ps => ps.maxHealth, healthRow, 1);
    public void UpgradeHealthX5() => TryUpgrade(data.health, ps => ps.maxHealth, healthRow, 5);
    public void UpgradeHealthX10() => TryUpgrade(data.health, ps => ps.maxHealth, healthRow, 10);

    public void UpgradeManaX1() => TryUpgrade(data.mana, ps => ps.maxMana, manaRow, 1);
    public void UpgradeManaX5() => TryUpgrade(data.mana, ps => ps.maxMana, manaRow, 5);
    public void UpgradeManaX10() => TryUpgrade(data.mana, ps => ps.maxMana, manaRow, 10);

    public void Close() => UIManager.Instance.HidePopupByType(PopupType.Stats);
}