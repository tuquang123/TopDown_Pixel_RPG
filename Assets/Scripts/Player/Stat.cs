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
            var finalValue = baseValue;
            foreach (var mod in modifiers)
            {
                finalValue += mod.value;
            }
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

[System.Serializable]
public class StatModifier
{
    public StatType statType;
    public float value;

    public StatModifier(StatType type, float value)
    {
        this.statType = type;
        this.value = value;
    }
}
