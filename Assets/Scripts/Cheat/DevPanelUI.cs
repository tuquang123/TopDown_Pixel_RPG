using System.Collections.Generic;
 using UnityEngine;
 using TMPro;
 using UnityEngine.UI;
 
 public class DevPanelUI : MonoBehaviour
 {
     [Header("References")]
  
     [SerializeField] private PlayerStats playerStats;
 
   
     [Header("Cheat Amount")]
     [SerializeField] private int cheatGoldAmount = 1000;
     [SerializeField] private int cheatGemAmount = 10;
     [SerializeField] private float cheatStatAmount = 5f;
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
     [SerializeField] private TextMeshProUGUI lifeStealText; // thêm vào Header "Texts"
     public void AddGold()
     {
         CurrencyManager.Instance.AddGold(cheatGoldAmount);
     }
 
     public void AddGems()
     {
         CurrencyManager.Instance.AddGems(cheatGemAmount);
     }
 
     public void AddDefense()
     {
         playerStats.defense.baseValue += cheatStatAmount;
         playerStats.CalculatePower();
         RefreshUI(); SaveGame();
     }
 
     public void AddCrit()
     {
         playerStats.critChance.baseValue += cheatStatAmount;
         playerStats.CalculatePower();
         RefreshUI(); SaveGame();
     }
 
     public void AddSpeed()
     {
         playerStats.speed.baseValue += 0.2f;// giảm từ 5 xuống 0.5
         playerStats.CalculatePower();
         RefreshUI(); SaveGame();
     }
 
     public void AddAttackSpeed()
     {
         playerStats.attackSpeed.baseValue += 0.2f;
         playerStats.CalculatePower();
         RefreshUI(); SaveGame();
     }
     public void AddMana()
     {
         playerStats.maxMana.baseValue += 20;
         playerStats.CalculatePower();
         RefreshUI(); SaveGame();
     }
     public void AddLevel()
     {
         var system = playerLevel.levelSystem;
 
         // Cho đủ EXP để level up 5 lần
         system.AddExp(system.ExpRequired * 1);
 
         // Không cần RefreshUI nếu PlayerLevelUI đã subscribe event
     }
     public void AddAttack()
     {
         playerStats.attack.baseValue += 5;
         playerStats.CalculatePower();
         RefreshUI(); SaveGame();
     }
 
     public void AddHP()
     {
         playerStats.maxHealth.baseValue += 2000;
         playerStats.CalculatePower();
         RefreshUI();

         SaveGame();
     }
 
     public void RefreshUI()
     {
         levelText.text = "Level: " + playerLevel.levelSystem.level;

         attackText.text      = "ATK: "       + playerStats.attack.Value;
         defenseText.text     = "DEF: "       + playerStats.defense.Value;
         critText.text        = "Crit: "      + playerStats.critChance.Value + "%";
         speedText.text       = "Speed: "     + playerStats.speed.Value;
         attackSpeedText.text = "AtkSpeed: "  + playerStats.attackSpeed.Value;
         lifeStealText.text   = "LifeSteal: " + playerStats.lifeSteal.Value + "%"; // ← thêm

         hpText.text   = "HP: "   + playerStats.maxHealth.Value;
         manaText.text = "Mana: " + playerStats.maxMana.Value;

         goldText.text = "Gold: " + CurrencyManager.Instance.Gold;
         gemText.text  = "Gem: "  + CurrencyManager.Instance.Gems;
     }
    
     private void Start()
     {
         statsGroup.SetActive(false);

         var system = playerLevel.levelSystem;
         system.OnLevelUp += HandleLevelUp;
         system.OnExpChanged += HandleExpChanged;

         CurrencyManager.Instance.OnGoldChanged += HandleGoldChanged;
         CurrencyManager.Instance.OnGemsChanged += HandleGemChanged;

         RefreshUI();
     }
     private void OnDestroy()
     {
         var system = playerLevel.levelSystem;
         system.OnLevelUp -= HandleLevelUp;
         system.OnExpChanged -= HandleExpChanged;
 
         if (CurrencyManager.Instance != null)
         {
             CurrencyManager.Instance.OnGoldChanged -= HandleGoldChanged;
             CurrencyManager.Instance.OnGemsChanged -= HandleGemChanged;
         }
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
     public void TogglePanel()
     {
         gameObject.SetActive(!gameObject.activeSelf);
     }
     private void HandleGoldChanged(int value)
     {
         RefreshUI();
     }
 
     private void HandleGemChanged(int value)
     {
         RefreshUI();
     }
    
     [SerializeField] private QuestDatabase questDatabase;
     public void CheatCompleteOneQuest()
     {
         QuestManager.Instance.DevQuestStep();
         RefreshUI();
         SaveGame();
     }
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
     [SerializeField] private GameObject statsGroup;
     public void ToggleStats()
     {
         statsGroup.SetActive(!statsGroup.activeSelf);
     }
     public void AddLifeSteal()
     {
         playerStats.lifeSteal.baseValue += cheatStatAmount;
         playerStats.CalculatePower();
         RefreshUI(); SaveGame();
     }
     public void HealFull()
     {
         if (PlayerStats.Instance == null) return;

         PlayerStats.Instance.Heal((int)PlayerStats.Instance.maxHealth.Value);
         PlayerStats.Instance.RestoreMana((int)PlayerStats.Instance.maxMana.Value);

         RefreshUI();
     }
 }