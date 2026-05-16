using UnityEngine;
using UnityEngine.UI;

public class EndGameUI : MonoBehaviour, IGameEventListener
{
    public GameObject endGamePanel;
    public Button retryButton;
    public Transform player;

    private PlayerStats playerStats;

    private void OnEnable()
    {
        GameEvents.OnPlayerDied.RegisterListener(this);
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerDied.UnregisterListener(this);
    }

    void Start()
    {
        endGamePanel.SetActive(false);
        retryButton.onClick.AddListener(RespawnPlayer);

        playerStats = player.GetComponent<PlayerStats>();
    }

    public void OnEventRaised() 
    {
        ShowEndGame();
    }

    private void ShowEndGame()
    {
        endGamePanel.SetActive(true);
    }

    private void RespawnPlayer()
    {
        endGamePanel.SetActive(false);
        player.position = Vector3.zero;
        playerStats.Revive();
    }
}