using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatsData", menuName = "Data/PlayerStatsData")]
public class PlayerStatsDataSO : ScriptableObject
{
    public PlayerStatsDataContainer stats = new PlayerStatsDataContainer();

    public void Save()
    {
        PlayerPrefs.SetInt("stat_attack_lv",      stats.attack.level);
        PlayerPrefs.SetInt("stat_defense_lv",     stats.defense.level);
        PlayerPrefs.SetInt("stat_speed_lv",       stats.speed.level);
        PlayerPrefs.SetInt("stat_crit_lv",        stats.crit.level);
        PlayerPrefs.SetInt("stat_lifesteal_lv",   stats.lifesteal.level);
        PlayerPrefs.SetInt("stat_attackspeed_lv", stats.attackSpeed.level);
        PlayerPrefs.SetInt("stat_health_lv",      stats.health.level);
        PlayerPrefs.SetInt("stat_mana_lv",        stats.mana.level);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        stats.attack.level      = PlayerPrefs.GetInt("stat_attack_lv",      0);
        stats.defense.level     = PlayerPrefs.GetInt("stat_defense_lv",     0);
        stats.speed.level       = PlayerPrefs.GetInt("stat_speed_lv",       0);
        stats.crit.level        = PlayerPrefs.GetInt("stat_crit_lv",        0);
        stats.lifesteal.level   = PlayerPrefs.GetInt("stat_lifesteal_lv",   0);
        stats.attackSpeed.level = PlayerPrefs.GetInt("stat_attackspeed_lv", 0);
        stats.health.level      = PlayerPrefs.GetInt("stat_health_lv",      0);
        stats.mana.level        = PlayerPrefs.GetInt("stat_mana_lv",        0);
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
    public PlayerStatData speed       = new PlayerStatData(3,   0.001f, 500);  // +0.01
    public PlayerStatData crit        = new PlayerStatData(5,   0.01f,  300);  // +0.1
    public PlayerStatData lifesteal   = new PlayerStatData(2,   0.01f,  200);  // +0.1
    public PlayerStatData attackSpeed = new PlayerStatData(1,   0.001f, 50);  // +0.01
    public PlayerStatData health      = new PlayerStatData(100, 10f,   80);
    public PlayerStatData mana        = new PlayerStatData(50,  5f,    60);
}

[Serializable]
public class PlayerStatData
{
    public float baseValue;
    public int   level;

    public float increasePerLevel;
    public int   goldCost;

    public PlayerStatData(float baseValue, float increasePerLevel, int goldCost)
    {
        this.baseValue        = baseValue;
        this.increasePerLevel = increasePerLevel;
        this.goldCost         = goldCost;
        this.level            = 0;
    }

    public float GetValue()                  => baseValue + level * increasePerLevel;
    public long  GetUpgradeCost(int atLevel) => (long)goldCost * (atLevel + 1);
    public void  Upgrade()                   => level++;
}