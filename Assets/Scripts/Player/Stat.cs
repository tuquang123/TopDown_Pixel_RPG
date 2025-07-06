using System.Collections.Generic;

[System.Serializable]
public class Stat
{
    public float baseValue;
    private List<StatModifier> modifiers = new List<StatModifier>();

    public Stat(float baseValue)
    {
        this.baseValue = baseValue;
    }

    public float Value
    {
        get
        {
            float finalValue = baseValue;
            float percentSum = 0f;

            foreach (var mod in modifiers)
            {
                if (mod.modType == StatModType.Flat)
                    finalValue += mod.value;
                else if (mod.modType == StatModType.Percent)
                    percentSum += mod.value;
            }

            finalValue *= (1 + percentSum / 100f);
            return finalValue;
        }
    }


    public void AddModifier(StatModifier modifier)
    {
        modifiers.Add(modifier);
    }

    public void RemoveModifier(StatModifier modifier)
    {
        modifiers.Remove(modifier);
    }
}
public enum StatType
{
    MaxHealth,
    MaxMana,
    Attack,
    Defense,
    Speed,
    CritChance,
    LifeSteal,
    AttackSpeed
}

public enum StatModType
{
    Flat,
    Percent
}

[System.Serializable]
public class StatModifier
{
    public StatType statType;
    public float value;
    public StatModType modType;

    public StatModifier(StatType type, float value, StatModType modType = StatModType.Percent)
    {
        this.statType = type;
        this.value = value;
        this.modType = modType;
    }
}

public class MultiStatModifier
{
    public List<StatModifier> modifiers;

    public MultiStatModifier(List<StatModifier> modifiers)
    {
        this.modifiers = modifiers;
    }
}

