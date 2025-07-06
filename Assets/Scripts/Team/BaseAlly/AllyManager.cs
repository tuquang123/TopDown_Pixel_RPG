using System.Collections.Generic;
using UnityEngine;

public interface IBuffableStats
{
    void ModifyAttack(float amount, float duration);
    void ModifyDefense(float amount, float duration);
    void ModifySpeed(float amount, float duration);
}

public class AllyManager : Singleton<AllyManager>
{
    private List<AllyBaseAI> activeAllies = new();

    public void RegisterAlly(AllyBaseAI ally)
    {
        if (!activeAllies.Contains(ally))
        {
            activeAllies.Add(ally);
        }
    }

    public void UnregisterAlly(AllyBaseAI ally)
    {
        if (activeAllies.Contains(ally))
        {
            activeAllies.Remove(ally);
        }
    }

    public int GetSlotIndex(AllyBaseAI ally)
    {
        return activeAllies.IndexOf(ally);
    }

    public Vector2 GetOffsetPosition(AllyBaseAI ally)
    {
        int index = GetSlotIndex(ally);
        float angle = 60f * index; 
        float radius = 1.2f; 

        return new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;
    }
    
    public IReadOnlyList<AllyBaseAI> GetAllies()
    {
        return activeAllies;
    }
    
    public void Clear()
    {
        activeAllies.Clear();
    }
}