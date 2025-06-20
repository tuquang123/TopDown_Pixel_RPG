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
public interface IGameEventListener
{
    void OnEventRaised();
}

public class GameEvent
{
    private readonly List<IGameEventListener> listeners = new();

    public void RegisterListener(IGameEventListener listener)
    {
        if (!listeners.Contains(listener))
            listeners.Add(listener);
    }

    public void UnregisterListener(IGameEventListener listener)
    {
        listeners.Remove(listener);
    }

    public void Raise()
    {
        foreach (var listener in listeners)
        {
            listener.OnEventRaised();
        }
    }
}

public static class GameEvents
{
    
    public static GameEvent OnUpdateAnimation = new(); 
    public static GameEvent<string> OnDeloyTeamAssist = new(); 
    public static GameEvent OnPlayerDied = new();
    public static GameEvent OnResetGame = new();
    public static GameEvent<string> OnShowToast = new();

}
