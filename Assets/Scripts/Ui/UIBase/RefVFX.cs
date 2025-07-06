using UnityEngine;

public class RefVFX : Singleton<RefVFX>
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
    
    
    public EnemyLevelDatabase levelDatabase;
    public GameObject arrowPrefab;
    public GameObject spellProjectilePrefab;
}
