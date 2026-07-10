using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; 
public class PlayerStatsPopupUI : BasePopup
{
    private const string PreviousStatRequiredMessage = "Requires previous stat upgrade";

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

    [Header("Insufficient Gold Toast Limit")]
    [SerializeField] private int maxInsufficientGoldToasts = 3;
    [SerializeField] private float insufficientGoldStreakResetGap = 0.5f;

    private PlayerStatsDataContainer data => dataAsset.stats;
    private int currentMultiplier = 1;

    private int insufficientGoldStreak = 0;
    private float lastInsufficientGoldTime = -999f;

    public override void Show()
    {
        base.Show();
        dataAsset.Load();
        ApplyBaseStatsOnly();
        SetMultiplier(1);
        insufficientGoldStreak = 0;
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

        float nextValue  = currentValue + times * stat.increasePerLevel;
        int   currentLvl = stat.GetLevelFromValue(currentValue);
        int   nextLvl    = currentLvl + times;

        string current = FormatStatValue(currentValue, isPercent);
        string next    = FormatStatValue(nextValue,    isPercent);

        text.text = $"<color=#AAAAAA>Lv.{currentLvl}</color>  {current} <color=#888888>>></color> <color=#00FF99>{next}</color>  <color=#AAAAAA>(to Lv.{nextLvl})</color>";
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

        if (!CanUpgradeStat(stat, out _))
        {
            text.enableAutoSizing = false;
            text.enableWordWrapping = false;
            text.text = "<color=#FFB84D>Locked</color>";
            return;
        }

        text.enableAutoSizing = false;
        text.enableWordWrapping = false;

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

        ps.attack.SetBaseValue(data.attack.GetValueAtLevel(dataAsset.GetSavedLevel(data.attack)));
        ps.defense.SetBaseValue(data.defense.GetValueAtLevel(dataAsset.GetSavedLevel(data.defense)));
        ps.speed.SetBaseValue(data.speed.GetValueAtLevel(dataAsset.GetSavedLevel(data.speed)));
        ps.critChance.SetBaseValue(data.crit.GetValueAtLevel(dataAsset.GetSavedLevel(data.crit)));
        ps.lifeSteal.SetBaseValue(data.lifesteal.GetValueAtLevel(dataAsset.GetSavedLevel(data.lifesteal)));
        ps.attackSpeed.SetBaseValue(data.attackSpeed.GetValueAtLevel(dataAsset.GetSavedLevel(data.attackSpeed)));

        float newMaxHp = data.health.GetValueAtLevel(dataAsset.GetSavedLevel(data.health));
        ps.maxHealth.SetBaseValue(newMaxHp);
        ps.currentHealth = Mathf.Clamp(ps.currentHealth, 1, (int)ps.maxHealth.Value);
        ps.NotifyHealthChanged();

        float newMaxMana = data.mana.GetValueAtLevel(dataAsset.GetSavedLevel(data.mana));
        ps.maxMana.SetBaseValue(newMaxMana);
        ps.currentMana = Mathf.Clamp(ps.currentMana, 0, (int)ps.maxMana.Value);
        ps.NotifyManaChanged();

        ps.CalculatePower();
        ps.NotifyStatsChanged();
    }


    private long CalculateTotalCost(PlayerStatData stat, int times)
    {
        var ps = PlayerStats.Instance;
        if (ps == null) return 0;

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

    private void TryUpgrade(PlayerStatData stat, System.Func<PlayerStats, Stat> getter, int times, Transform animTarget = null)
    {
        if (!CanUpgradeStat(stat, out PlayerStatData requiredStat))
        {
            GameEvents.OnShowToast.Raise(GetPreviousStatRequiredText(requiredStat));
            return;
        }

        long totalCost = CalculateTotalCost(stat, times);
        int  safeCost  = totalCost > int.MaxValue ? int.MaxValue : (int)totalCost;

        if (!CurrencyManager.Instance.SpendGold(safeCost))
        {
            ShowInsufficientGoldToastLimited();
            return;
        }

        insufficientGoldStreak = 0;

        var ps           = PlayerStats.Instance;
        var playerStat   = getter(ps);
        int currentLevel = stat.GetLevelFromValue(playerStat.baseValue);
        int nextLevel    = currentLevel + times;

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

     
        if (animTarget != null)
        {
            animTarget.DOKill();
            animTarget.localScale = Vector3.one;
            animTarget.DOPunchScale(Vector3.one * 0.25f, 0.4f, 8, 0.5f);
        }
    }

    private void ShowInsufficientGoldToastLimited()
    {
        float now = Time.unscaledTime;

        if (now - lastInsufficientGoldTime > insufficientGoldStreakResetGap)
            insufficientGoldStreak = 0;

        lastInsufficientGoldTime = now;
        insufficientGoldStreak++;

        if (insufficientGoldStreak <= maxInsufficientGoldToasts)
            GameEvents.OnShowToast.Raise("Not enough Gold");
    }

    private bool CanUpgradeStat(PlayerStatData stat, out PlayerStatData requiredStat)
    {
        requiredStat = GetRequiredPreviousStat(stat);
        if (requiredStat == null)
            return true;

        return dataAsset.GetSavedLevel(requiredStat) > 0;
    }

    private PlayerStatData GetRequiredPreviousStat(PlayerStatData stat)
    {
        if (stat == data.defense)     return data.attack;
        if (stat == data.health)      return data.defense;
        if (stat == data.mana)        return data.health;
        if (stat == data.speed)       return data.mana;
        if (stat == data.attackSpeed) return data.speed;
        if (stat == data.crit)        return data.attackSpeed;
        if (stat == data.lifesteal)   return data.crit;

        return null;
    }

    private string GetPreviousStatRequiredText(PlayerStatData requiredStat)
    {
        string requiredName = GetStatDisplayName(requiredStat);
        return string.IsNullOrEmpty(requiredName)
            ? PreviousStatRequiredMessage
            : $"{PreviousStatRequiredMessage}: {requiredName}";
    }

    private string GetStatDisplayName(PlayerStatData stat)
    {
        if (stat == data.attack)      return "Attack";
        if (stat == data.defense)     return "Defense";
        if (stat == data.health)      return "HP";
        if (stat == data.mana)        return "Mana";
        if (stat == data.speed)       return "Speed";
        if (stat == data.attackSpeed) return "Atk Speed";
        if (stat == data.crit)        return "Crit";
        if (stat == data.lifesteal)   return "Life Steal";

        return "";
    }

    public void OnClickX1()   => SetMultiplier(1);
    public void OnClickX10()  => SetMultiplier(10);
    public void OnClickX100() => SetMultiplier(100);
    public void UpgradeAttack()      => TryUpgrade(data.attack,      ps => ps.attack,      currentMultiplier, attackPreview?.transform);
    public void UpgradeDefense()     => TryUpgrade(data.defense,     ps => ps.defense,     currentMultiplier, defensePreview?.transform);
    public void UpgradeSpeed()       => TryUpgrade(data.speed,       ps => ps.speed,       currentMultiplier, speedPreview?.transform);
    public void UpgradeCrit()        => TryUpgrade(data.crit,        ps => ps.critChance,  currentMultiplier, critPreview?.transform);
    public void UpgradeLifesteal()   => TryUpgrade(data.lifesteal,   ps => ps.lifeSteal,   currentMultiplier, lifestealPreview?.transform);
    public void UpgradeAttackSpeed() => TryUpgrade(data.attackSpeed, ps => ps.attackSpeed, currentMultiplier, attackSpeedPreview?.transform);
    public void UpgradeHealth()      => TryUpgrade(data.health,      ps => ps.maxHealth,   currentMultiplier, healthPreview?.transform);
    public void UpgradeMana()        => TryUpgrade(data.mana,        ps => ps.maxMana,     currentMultiplier, manaPreview?.transform);
    public void Close() => UIManager.Instance.HidePopupByType(PopupType.Stats);
}
