using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatsData", menuName = "Data/PlayerStatsData")]
public class PlayerStatsDataSO : ScriptableObject
{
    public PlayerStatsDataContainer stats = new PlayerStatsDataContainer();

    [System.NonSerialized] private PlayerStatUpgradeSaveData runtimeLevels = new PlayerStatUpgradeSaveData();

    public void Save() { }
    public void Load() { }

    public int GetSavedLevel(PlayerStatData stat) => runtimeLevels.GetLevel(stat, stats);
    public void SetSavedLevel(PlayerStatData stat, int level) => runtimeLevels.SetLevel(stat, stats, level);

    public PlayerStatUpgradeSaveData CaptureFromRuntime(PlayerStats ps)
    {
        return new PlayerStatUpgradeSaveData
        {
            attack = stats.attack.GetLevelFromValue(ps.attack.baseValue),
            defense = stats.defense.GetLevelFromValue(ps.defense.baseValue),
            speed = stats.speed.GetLevelFromValue(ps.speed.baseValue),
            crit = stats.crit.GetLevelFromValue(ps.critChance.baseValue),
            lifesteal = stats.lifesteal.GetLevelFromValue(ps.lifeSteal.baseValue),
            attackSpeed = stats.attackSpeed.GetLevelFromValue(ps.attackSpeed.baseValue),
            health = stats.health.GetLevelFromValue(ps.maxHealth.baseValue),
            mana = stats.mana.GetLevelFromValue(ps.maxMana.baseValue)
        };
    }

    public void ApplySaveDataToRuntime(PlayerStats ps, PlayerStatUpgradeSaveData saveData)
    {
        runtimeLevels = saveData ?? new PlayerStatUpgradeSaveData();
        ApplyLevelToStat(ps.attack, stats.attack, runtimeLevels.attack);
        ApplyLevelToStat(ps.defense, stats.defense, runtimeLevels.defense);
        ApplyLevelToStat(ps.speed, stats.speed, runtimeLevels.speed);
        ApplyLevelToStat(ps.critChance, stats.crit, runtimeLevels.crit);
        ApplyLevelToStat(ps.lifeSteal, stats.lifesteal, runtimeLevels.lifesteal);
        ApplyLevelToStat(ps.attackSpeed, stats.attackSpeed, runtimeLevels.attackSpeed);
        ApplyLevelToStat(ps.maxHealth, stats.health, runtimeLevels.health);
        ApplyLevelToStat(ps.maxMana, stats.mana, runtimeLevels.mana);
        ps.currentHealth = Mathf.Clamp(ps.currentHealth, 1, (int)ps.maxHealth.Value);
        ps.currentMana = Mathf.Clamp(ps.currentMana, 0, (int)ps.maxMana.Value);
        ps.NotifyHealthChanged();
        ps.NotifyManaChanged();
        ps.NotifyStatsChanged();
    }

    private static void ApplyLevelToStat(Stat runtimeStat, PlayerStatData config, int level)
    {
        runtimeStat.SetBaseValue(config.GetValueAtLevel(level));
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
public class PlayerStatUpgradeSaveData
{
    public int attack;
    public int defense;
    public int speed;
    public int crit;
    public int lifesteal;
    public int attackSpeed;
    public int health;
    public int mana;

    public int GetLevel(PlayerStatData stat, PlayerStatsDataContainer data)
    {
        if (stat == data.attack) return attack;
        if (stat == data.defense) return defense;
        if (stat == data.speed) return speed;
        if (stat == data.crit) return crit;
        if (stat == data.lifesteal) return lifesteal;
        if (stat == data.attackSpeed) return attackSpeed;
        if (stat == data.health) return health;
        if (stat == data.mana) return mana;
        return 0;
    }

    public void SetLevel(PlayerStatData stat, PlayerStatsDataContainer data, int level)
    {
        level = Mathf.Max(0, level);
        if (stat == data.attack) attack = level;
        else if (stat == data.defense) defense = level;
        else if (stat == data.speed) speed = level;
        else if (stat == data.crit) crit = level;
        else if (stat == data.lifesteal) lifesteal = level;
        else if (stat == data.attackSpeed) attackSpeed = level;
        else if (stat == data.health) health = level;
        else if (stat == data.mana) mana = level;
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
