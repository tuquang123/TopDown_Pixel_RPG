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
        stats.speed.increasePerLevel       = 0.05f;
        stats.attackSpeed.increasePerLevel = 0.05f;
        stats.crit.increasePerLevel        = 0.1f;
        stats.lifesteal.increasePerLevel   = 0.1f;

        stats.attack.goldCost      = 50;
        stats.defense.goldCost     = 40;
        stats.speed.goldCost       = 500;
        stats.crit.goldCost        = 300;
        stats.lifesteal.goldCost   = 200;
        stats.attackSpeed.goldCost = 50;
        stats.health.goldCost      = 80;
        stats.mana.goldCost        = 60;
    }
}

[Serializable]
public class PlayerStatsDataContainer
{
    public PlayerStatData attack      = new PlayerStatData(10,  2f,    50);
    public PlayerStatData defense     = new PlayerStatData(5,   1f,    40);
    public PlayerStatData speed       = new PlayerStatData(3,   0.05f, 500);
    public PlayerStatData crit        = new PlayerStatData(5,   0.1f,  300);
    public PlayerStatData lifesteal   = new PlayerStatData(2,   0.1f,  200);
    public PlayerStatData attackSpeed = new PlayerStatData(1,   0.05f, 50);
    public PlayerStatData health      = new PlayerStatData(100, 10f,   80);
    public PlayerStatData mana        = new PlayerStatData(50,  5f,    60);
}

[Serializable]
public class PlayerStatData
{
    public float baseValue;
    public float increasePerLevel;
    public int   goldCost;

    public PlayerStatData(float baseValue, float increasePerLevel, int goldCost)
    {
        this.baseValue        = baseValue;
        this.increasePerLevel = increasePerLevel;
        this.goldCost         = goldCost;
    }

    // Không lưu/đọc level trong data SO; level nâng cấp phải suy ra từ runtime value hiện tại.
    public int GetLevelFromValue(float runtimeValue)
    {
        if (increasePerLevel <= 0f) return 0;
        return Mathf.Max(0, Mathf.RoundToInt((runtimeValue - baseValue) / increasePerLevel));
    }

    public float GetValueAtLevel(int level) => baseValue + Mathf.Max(0, level) * increasePerLevel;

    public long GetUpgradeCost(int atLevel) => (long)goldCost * (atLevel + 1);
}
