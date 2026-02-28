using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DevPanelUI : MonoBehaviour
{
    [Header("References")]
 
    [SerializeField] private PlayerStats playerStats;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI hpText;
    

    public void AddLevel()
    {
        var system = playerLevel.levelSystem;

        // Cho đủ EXP để level up 5 lần
        system.AddExp(system.ExpRequired * 5);

        // Không cần RefreshUI nếu PlayerLevelUI đã subscribe event
    }
    public void AddAttack()
    {
        playerStats.attack.baseValue += 5;
        playerStats.CalculatePower();
        RefreshUI();
    }

    public void AddHP()
    {
        playerStats.maxHealth.baseValue += 20;
        playerStats.CalculatePower();
        RefreshUI();
    }

    public void RefreshUI()
    {
        levelText.text = "Level: " + playerLevel.levelSystem.level;
        attackText.text = "ATK: " + playerStats.attack.Value;
        hpText.text = "HP: " + playerStats.maxHealth.Value;
    }
   
    private void Start()
    {
        var system = playerLevel.levelSystem;
        system.OnLevelUp += HandleLevelUp;
        system.OnExpChanged += HandleExpChanged;

        RefreshUI();
    }

    private void OnDestroy()
    {
        var system = playerLevel.levelSystem;
        system.OnLevelUp -= HandleLevelUp;
        system.OnExpChanged -= HandleExpChanged;
    }

    private void HandleLevelUp(int level)
    {
        RefreshUI();
    }

    private void HandleExpChanged(float current, float required)
    {
        RefreshUI();
    }
    private PlayerLevel playerLevel;

    private void Awake()
    {
        playerLevel = FindObjectOfType<PlayerLevel>();
    }
}