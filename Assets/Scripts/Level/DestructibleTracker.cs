using System.Collections.Generic;
using UnityEngine;

public class DestructibleTracker : Singleton<DestructibleTracker>
{
    private List<DestructibleObject> destructibles = new List<DestructibleObject>();

    public void Register(DestructibleObject obj)
    {
        if (!destructibles.Contains(obj))
            destructibles.Add(obj);
    }

    public void Unregister(DestructibleObject obj)
    {
        destructibles.Remove(obj);
    }

    public List<DestructibleObject> GetInRange(Vector2 position, float range)
    {
        List<DestructibleObject> result = new List<DestructibleObject>();

        foreach (var obj in destructibles)
        {
            if (obj == null) continue;

            float dist = Vector2.Distance(position, obj.transform.position);
            if (dist <= range)
                result.Add(obj);
        }

        return result;
    }
}