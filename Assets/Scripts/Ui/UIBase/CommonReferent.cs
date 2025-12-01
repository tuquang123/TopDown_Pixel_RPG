using UnityEngine;
using UnityEngine.Serialization;

public class CommonReferent : Singleton<CommonReferent>
{
    public GameObject goldPrefab;
    public GameObject gemPrefab;
    public GameObject playerPrefab;
    public GameObject bloodVfxPrefab;
    public GameObject destructionVFXPrefab;
    public GameObject hitVFXPrefab;
    public BossHealthUI bossHealthUI;
    public GameObject  canvasHp;
    public GameObject  hpSliderUi;
    
    public ItemDatabase itemDatabase;
    public Inventory inventory;
    public PlayerStats playerStats;
    public Equipment equipment;
    public SkillSystem skill;
    public PlayerLevel playerLevel;
    public PlayerLevelUI playerLevelUI;
    
    public EnemyLevelDatabase enemyLevelDatabase;
    public LevelDatabase levelDatabase;
    public GameObject arrowPrefab;
    public GameObject spellProjectilePrefab;
    public GameObject dialogBtn;
    public GameObject dialogShopBtn;
    public Sprite iconGold;
    public Sprite iconExp;
    public GameObject itemDropPrefab;
    public GameObject deadVFXPrefab;
    
    public float spawnRange = 12f;       
    public float keepAliveRange = 20f; 
}
