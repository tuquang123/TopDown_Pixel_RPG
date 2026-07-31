using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatsData", menuName = "Data/PlayerStatsData")]
public class PlayerStatsDataSO : ScriptableObject
{
    public PlayerStatsDataContainer stats = new PlayerStatsDataContainer();

    public void Save() => PlayerPrefs.Save();
    public void Load() { }

    public int GetSavedLevel(PlayerStatData stat)
    {
        return Mathf.Max(0, PlayerPrefs.GetInt(GetStatLevelKey(stat), 0));
    }

    public void SetSavedLevel(PlayerStatData stat, int level)
    {
        PlayerPrefs.SetInt(GetStatLevelKey(stat), Mathf.Max(0, level));
    }

    private string GetStatLevelKey(PlayerStatData stat)
    {
        if (stat == stats.attack)      return "stat_attack_lv";
        if (stat == stats.defense)     return "stat_defense_lv";
        if (stat == stats.speed)       return "stat_speed_lv";
        if (stat == stats.crit)        return "stat_crit_lv";
        if (stat == stats.lifesteal)   return "stat_lifesteal_lv";
        if (stat == stats.attackSpeed) return "stat_attackspeed_lv";
        if (stat == stats.health)      return "stat_health_lv";
        if (stat == stats.mana)        return "stat_mana_lv";

        return "stat_unknown_lv";
    }

    private void OnValidate()
    {
        stats.attack.increasePerLevel      = 3f;
        stats.defense.increasePerLevel     = 1.2f;
        stats.speed.increasePerLevel       = 0.035f;
        stats.attackSpeed.increasePerLevel = 0.04f;
        stats.crit.increasePerLevel        = 1f;
        stats.lifesteal.increasePerLevel   = 1f;
        stats.health.increasePerLevel      = 15f;
        stats.mana.increasePerLevel        = 8f;

        stats.attack.goldCost      = 20;
        stats.defense.goldCost     = 18;
        stats.speed.goldCost       = 35;
        stats.crit.goldCost        = 50;
        stats.lifesteal.goldCost   = 60;
        stats.attackSpeed.goldCost = 28;
        stats.health.goldCost      = 25;
        stats.mana.goldCost        = 20;

        stats.attack.costGrowthRate      = 1.035f;
        stats.defense.costGrowthRate     = 1.035f;
        stats.speed.costGrowthRate       = 1.038f;
        stats.crit.costGrowthRate        = 1.04f;
        stats.lifesteal.costGrowthRate   = 1.04f;
        stats.attackSpeed.costGrowthRate = 1.038f;
        stats.health.costGrowthRate      = 1.035f;
        stats.mana.costGrowthRate        = 1.035f;
    }
}

[Serializable]
public class PlayerStatsDataContainer
{
    public PlayerStatData attack      = new PlayerStatData(14,  3f,     20, 1.035f);
    public PlayerStatData defense     = new PlayerStatData(5,   1.2f,   18, 1.035f);
    public PlayerStatData speed       = new PlayerStatData(2.2f, 0.035f, 35, 1.038f);
    public PlayerStatData crit        = new PlayerStatData(5,   1f,     50, 1.04f);
    public PlayerStatData lifesteal   = new PlayerStatData(0,   1f,     60, 1.04f);
    public PlayerStatData attackSpeed = new PlayerStatData(1,   0.04f,  28, 1.038f);
    public PlayerStatData health      = new PlayerStatData(120, 15f,    25, 1.035f);
    public PlayerStatData mana        = new PlayerStatData(60,  8f,     20, 1.035f);
}

[Serializable]
public class PlayerStatData
{
    public float baseValue;
    public float increasePerLevel;
    public int   goldCost;
    public float costGrowthRate;

    public PlayerStatData(float baseValue, float increasePerLevel, int goldCost, float costGrowthRate = 1.08f)
    {
        this.baseValue        = baseValue;
        this.increasePerLevel = increasePerLevel;
        this.goldCost         = goldCost;
        this.costGrowthRate   = Mathf.Max(1f, costGrowthRate);
    }

    // Không lưu/đọc level trong data SO; level nâng cấp phải suy ra từ runtime value hiện tại.
    public int GetLevelFromValue(float runtimeValue)
    {
        if (increasePerLevel <= 0f) return 0;
        return Mathf.Max(0, Mathf.RoundToInt((runtimeValue - baseValue) / increasePerLevel));
    }

    public float GetValueAtLevel(int level) => baseValue + Mathf.Max(0, level) * increasePerLevel;

    // Cost kiểu idle phổ biến: tăng theo cấp số nhân nhẹ để giữ nhịp tiến trình dài hạn.
    // atLevel = level hiện tại của stat trước khi mua lần nâng tiếp theo.
    public long GetUpgradeCost(int atLevel)
    {
        int safeLevel = Mathf.Max(0, atLevel);
        double scaled = goldCost * System.Math.Pow(costGrowthRate, safeLevel);
        return (long)System.Math.Ceiling(scaled);
    }
}
