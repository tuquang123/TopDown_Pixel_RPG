using UnityEngine;
using System;

[Serializable]
public class LevelSystem
{
    public int level = 1;
    public float exp = 0;
    public float baseExp = 100f;
    public float growthFactor = 1.5f;
    public int skillPoints = 0;

    public float ExpRequired => baseExp * Mathf.Pow(level, growthFactor);

    public event Action<int> OnLevelUp;
    public event Action<float, float> OnExpChanged; // current, required

    public void AddExp(float amount)
    {
        exp += amount;
        OnExpChanged?.Invoke(exp, ExpRequired);
        while (exp >= ExpRequired)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        exp -= ExpRequired;
        level++;
        skillPoints++;
        OnLevelUp?.Invoke(level);
    }
}