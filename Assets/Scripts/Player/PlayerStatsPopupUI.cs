using TMPro;
using UnityEngine;

public class PlayerStatsPopupUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statsText;

    private void Start()
    {
        Show(PlayerStats.Instance);
    }

    private void OnEnable()
    {
        Show(PlayerStats.Instance); 
    }
    public void Show(PlayerStats stats)
    {
        gameObject.SetActive(true);

        statsText.text =
            $"<color=#FFD700>Level:</color> {stats.level}\n" +
            $"<color=#FFD700>HP:</color> {stats.maxHealth.Value}\n" +
            $"<color=#FFD700>MP:</color> {stats.maxMana.Value}\n" +
            $"<color=#FFD700>Attack:</color> {stats.attack.Value}\n" +
            $"<color=#FFD700>Defense:</color> {stats.defense.Value}\n" +
            $"<color=#FFD700>Speed:</color> {stats.speed.Value}\n" +
            $"<color=#FFD700>Crit:</color> {stats.critChance.Value}%\n" +
            $"<color=#FFD700>LifeSteal:</color> {stats.lifeSteal.Value}%";
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}