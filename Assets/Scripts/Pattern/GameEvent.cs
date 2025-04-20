using System.Collections.Generic;

public interface IGameEventListener<T>
{
    void OnEventRaised(T value);
}

public class GameEvent<T>
{
    private readonly List<IGameEventListener<T>> listeners = new();

    public void RegisterListener(IGameEventListener<T> listener)
    {
        if (!listeners.Contains(listener))
            listeners.Add(listener);
    }

    public void UnregisterListener(IGameEventListener<T> listener)
    {
        listeners.Remove(listener);
    }

    public void Raise(T value)
    {
        foreach (var listener in listeners)
        {
            listener.OnEventRaised(value);
        }
    }
}
public static class GameEvents
{
    public static GameEvent<EnemyAI> OnEnemyDefeated = new();
    public static GameEvent<int> OnPlayerTakeDamage = new();
    public static GameEvent<string> OnItemPickedUp = new();
}
