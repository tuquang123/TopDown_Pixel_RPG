using System.Collections.Generic;

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
        this.value    = value;
        this.modType  = modType;
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

[System.Serializable]
public class Stat
{
    public float baseValue;

    [System.NonSerialized]
    private List<StatModifier> modifiers;

    public Stat(float baseValue)
    {
        this.baseValue = baseValue;
        modifiers      = new List<StatModifier>();
    }

    public float Value
    {
        get
        {
            if (modifiers == null) modifiers = new List<StatModifier>();

            float finalValue  = baseValue;
            float percentSum  = 0f;

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

    public void SetBaseValue(float value) => baseValue = value;

    public void AddModifier(StatModifier modifier)
    {
        modifiers ??= new List<StatModifier>();
        modifiers.Add(modifier);
    }

    public void RemoveModifier(StatModifier modifier)
    {
        modifiers?.Remove(modifier);
    }

    public void RemoveAllModifiers() => modifiers?.Clear();
}