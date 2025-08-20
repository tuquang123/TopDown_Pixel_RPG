using UnityEngine;

public class CommonReferent : Singleton<CommonReferent>
{
    public GameObject goldPrefab;
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
    
    public EnemyLevelDatabase levelDatabase;
    public GameObject arrowPrefab;
    public GameObject spellProjectilePrefab;
}
