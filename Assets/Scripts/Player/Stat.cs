using System.Collections.Generic;

[System.Serializable]
public class Stat
{
    public int baseValue;
    private List<StatModifier> modifiers = new List<StatModifier>();

    public Stat(int baseValue)
    {
        this.baseValue = baseValue;
    }

    public int Value
    {
        get
        {
            int finalValue = baseValue;
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
    Speed
}

[System.Serializable]
public class StatModifier
{
    public StatType statType;
    public int value;

    public StatModifier(StatType type, int value)
    {
        this.statType = type;
        this.value = value;
    }
}
