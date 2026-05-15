using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatsData", menuName = "Data/PlayerStatsData")]
public class PlayerStatsDataSO : ScriptableObject
{
    public PlayerStatsDataContainer stats = new PlayerStatsDataContainer();

    public void Save()
    {
        PlayerPrefs.SetFloat("stat_attack_val",      stats.attack.currentValue);
        PlayerPrefs.SetFloat("stat_defense_val",     stats.defense.currentValue);
        PlayerPrefs.SetFloat("stat_speed_val",       stats.speed.currentValue);
        PlayerPrefs.SetFloat("stat_crit_val",        stats.crit.currentValue);
        PlayerPrefs.SetFloat("stat_lifesteal_val",   stats.lifesteal.currentValue);
        PlayerPrefs.SetFloat("stat_attackspeed_val", stats.attackSpeed.currentValue);
        PlayerPrefs.SetFloat("stat_health_val",      stats.health.currentValue);
        PlayerPrefs.SetFloat("stat_mana_val",        stats.mana.currentValue);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        stats.attack.currentValue      = PlayerPrefs.GetFloat("stat_attack_val",      stats.attack.baseValue);
        stats.defense.currentValue     = PlayerPrefs.GetFloat("stat_defense_val",     stats.defense.baseValue);
        stats.speed.currentValue       = PlayerPrefs.GetFloat("stat_speed_val",       stats.speed.baseValue);
        stats.crit.currentValue        = PlayerPrefs.GetFloat("stat_crit_val",        stats.crit.baseValue);
        stats.lifesteal.currentValue   = PlayerPrefs.GetFloat("stat_lifesteal_val",   stats.lifesteal.baseValue);
        stats.attackSpeed.currentValue = PlayerPrefs.GetFloat("stat_attackspeed_val", stats.attackSpeed.baseValue);
        stats.health.currentValue      = PlayerPrefs.GetFloat("stat_health_val",      stats.health.baseValue);
        stats.mana.currentValue        = PlayerPrefs.GetFloat("stat_mana_val",        stats.mana.baseValue);
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
    public float currentValue;
    public float increasePerLevel;
    public int   goldCost;

    public PlayerStatData(float baseValue, float increasePerLevel, int goldCost)
    {
        this.baseValue        = baseValue;
        this.currentValue     = baseValue;
        this.increasePerLevel = increasePerLevel;
        this.goldCost         = goldCost;
    }

    public int   Level           => Mathf.RoundToInt((currentValue - baseValue) / increasePerLevel);
    public float GetValue()      => currentValue;
    public void  Upgrade()       => currentValue += increasePerLevel;

    public long GetUpgradeCost(int atLevel) => (long)goldCost * (atLevel + 1);
}