using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DevPanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Cheat Amount")]
    [SerializeField] private int   cheatGoldAmount  = 1000;
    [SerializeField] private int   cheatGemAmount   = 10;
    [SerializeField] private float cheatStatAmount  = 5f;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI critText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI gemText;
    [SerializeField] private TextMeshProUGUI lifeStealText;

    [SerializeField] private GameObject    statsGroup;
    [SerializeField] private QuestDatabase questDatabase;

    private PlayerLevel playerLevel;

    private void Awake()
    {
        playerLevel = FindObjectOfType<PlayerLevel>();
    }

    private void Start()
    {
        statsGroup.SetActive(false);

        var system = playerLevel.levelSystem;
        system.OnLevelUp      += HandleLevelUp;
        system.OnExpChanged   += HandleExpChanged;

        CurrencyManager.Instance.OnGoldChanged += HandleGoldChanged;
        CurrencyManager.Instance.OnGemsChanged += HandleGemChanged;

        RefreshUI();
    }

    private void OnDestroy()
    {
        var system = playerLevel.levelSystem;
        system.OnLevelUp    -= HandleLevelUp;
        system.OnExpChanged -= HandleExpChanged;

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnGoldChanged -= HandleGoldChanged;
            CurrencyManager.Instance.OnGemsChanged -= HandleGemChanged;
        }
    }

    // ========================= CHEAT HELPERS =========================

    private void CheatStat(System.Action applyFn)
    {
        applyFn();
        playerStats.CalculatePower();
        playerStats.NotifyStatsChanged();
        RefreshUI();
        SaveGame();
    }

    // ========================= CHEAT BUTTONS =========================

    public void AddGold()  => CurrencyManager.Instance.AddGold(cheatGoldAmount);
    public void AddGems()  => CurrencyManager.Instance.AddGems(cheatGemAmount);

    public void AddAttack()      => CheatStat(() => playerStats.attack.baseValue      += cheatStatAmount);
    public void AddDefense()     => CheatStat(() => playerStats.defense.baseValue     += cheatStatAmount);
    public void AddCrit()        => CheatStat(() => playerStats.critChance.baseValue  += cheatStatAmount);
    public void AddLifeSteal()   => CheatStat(() => playerStats.lifeSteal.baseValue   += cheatStatAmount);
    public void AddSpeed()       => CheatStat(() => playerStats.speed.baseValue       += 0.2f);
    public void AddAttackSpeed() => CheatStat(() => playerStats.attackSpeed.baseValue += 0.2f);

    public void AddMana() => CheatStat(() =>
    {
        playerStats.maxMana.baseValue += 20;
        playerStats.currentMana = (int)playerStats.maxMana.Value;
        playerStats.NotifyManaChanged();
    });

    public void AddHP() => CheatStat(() =>
    {
        playerStats.maxHealth.baseValue += 2000;
        playerStats.currentHealth = (int)playerStats.maxHealth.Value;
        playerStats.NotifyHealthChanged();
    });

    public void AddLevel()
    {
        var system = playerLevel.levelSystem;
        system.AddExp(system.ExpRequired * 1);
    }

    public void HealFull()
    {
        if (PlayerStats.Instance == null) return;
        PlayerStats.Instance.Heal((int)PlayerStats.Instance.maxHealth.Value);
        PlayerStats.Instance.RestoreMana((int)PlayerStats.Instance.maxMana.Value);
        RefreshUI();
    }

    public void CheatCompleteOneQuest()
    {
        QuestManager.Instance.DevQuestStep();
        RefreshUI();
        SaveGame();
    }

    // ========================= UI =========================

    public void RefreshUI()
    {
        levelText.text       = "Level: "     + playerLevel.levelSystem.level;
        attackText.text      = "ATK: "       + playerStats.attack.Value;
        defenseText.text     = "DEF: "       + playerStats.defense.Value;
        critText.text        = "Crit: "      + playerStats.critChance.Value + "%";
        speedText.text       = "Speed: "     + playerStats.speed.Value;
        attackSpeedText.text = "AtkSpeed: "  + playerStats.attackSpeed.Value;
        lifeStealText.text   = "LifeSteal: " + playerStats.lifeSteal.Value + "%";
        hpText.text          = "HP: "        + playerStats.maxHealth.Value;
        manaText.text        = "Mana: "      + playerStats.maxMana.Value;
        goldText.text        = "Gold: "      + CurrencyManager.Instance.Gold;
        gemText.text         = "Gem: "       + CurrencyManager.Instance.Gems;
    }

    public void TogglePanel()  => gameObject.SetActive(!gameObject.activeSelf);
    public void ToggleStats()  => statsGroup.SetActive(!statsGroup.activeSelf);

    // ========================= EVENTS =========================

    private void HandleLevelUp(int level)       => RefreshUI();
    private void HandleExpChanged(float c, float r) => RefreshUI();
    private void HandleGoldChanged(int value)   => RefreshUI();
    private void HandleGemChanged(int value)    => RefreshUI();

    // ========================= SAVE =========================

    private void SaveGame()
    {
        SaveManager.Save(
            0,
            PlayerStats.Instance,
            FindObjectOfType<Inventory>(),
            FindObjectOfType<Equipment>(),
            FindObjectOfType<SkillSystem>(),
            FindObjectOfType<PlayerLevel>()
        );
    }
}