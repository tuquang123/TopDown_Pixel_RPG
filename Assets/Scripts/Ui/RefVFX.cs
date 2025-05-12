using UnityEngine;

public class RefVFX : Singleton<RefVFX>, IGameEventListener
{
    public GameObject goldPrefab;
    public GameObject playerPrefab;
    
    public void OnEventRaised() 
    {
        //playerPrefab = GameObject.FindGameObjectWithTag("Player");
    }
    private void OnEnable()
    {
        GameEvents.OnResetGame.RegisterListener(this);
    }

    private void OnDisable()
    {
        GameEvents.OnResetGame.UnregisterListener(this);
    }
}
