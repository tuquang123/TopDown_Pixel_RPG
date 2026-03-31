using UnityEngine;
using System.Collections;

public class DestructibleObject : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; set; }

    private EnemyHealthUI healthUI;

    [Header("Drop Settings")]
    [SerializeField] private int minGold = 1;
    [SerializeField] private int maxGold = 3;

    [SerializeField, Range(0f, 1f)] private float gemDropChance = 0.5f;
    [SerializeField] private int minGem = 1;
    [SerializeField] private int maxGem = 2;

    [Header("Enemy Spawn On Break")]
    [SerializeField] private bool spawnEnemyOnBreak = false;
    [SerializeField, Range(0f, 1f)] private float enemySpawnChance = 1f;   // 1 = luôn spawn
    [SerializeField] private bool useSpawnPoint = false;

    //[SerializeField] private SpawnPoint spawnPoint;      // nếu muốn dùng SpawnPoint
    [SerializeField] private GameObject[] enemyPrefabs;  // spawn trực tiếp

    [Header("HP Display Type")]
    [Tooltip("true = Luôn hiển thị HP\nfalse = Chỉ hiển thị khi bị đánh")]
    [SerializeField] private bool alwaysShowHP = false;
  
  
    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float shakeMagnitude = 0.05f;

    [Header("Respawn")]
    [SerializeField] private float respawnTime = 10f;

    private Vector3 originalPos;
    private Coroutine flashCoroutine;
   

    private void Awake()
    {
        originalPos = transform.localPosition;
    }

  
    public void ShowUIOnHit()
    {
        if (healthUI != null && !alwaysShowHP)
        {
            healthUI.ShowUI();
            healthUI.UpdateHealth(currentHealth);
        }
    }

    private void OnDisable()
    {
        DestructibleTracker.Instance?.Unregister(this);
    }

    public void Hit()
    {
        currentHealth--;
        FloatingTextSpawner.Instance.SpawnText("-1", transform.position + Vector3.up * 1.2f, Color.white);

        // Hiển thị HP khi bị đánh (nếu là loại chỉ hiện khi hit)
        ShowUIOnHit();

        healthUI?.UpdateHealth(currentHealth);

        PlayHitFlash();
        StartCoroutine(Shake());

        if (currentHealth <= 0)
            HandleDestruction();
    }
    

    private void PlayHitFlash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(HitFlashRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    private IEnumerator Shake()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            Vector3 randomOffset = Random.insideUnitCircle * shakeMagnitude;
            transform.localPosition = originalPos + new Vector3(randomOffset.x, randomOffset.y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPos;
    }
    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnTime);

        currentHealth = 0;
        spriteRenderer.color = Color.white;
        transform.localPosition = originalPos;

        gameObject.SetActive(true);
        DestructibleTracker.Instance?.Register(this);
    }
    [Header("Selection")]
    [SerializeField] private GameObject selectionCircle;

    [SerializeField] public string displayName = "Barrier"; // default tên

    public void SetSelected(bool value)
    {
        if (selectionCircle != null)
            selectionCircle.SetActive(value);
    }
    private void Start()
    {
        CreateHealthUI();
    }

    private void CreateHealthUI()
    {
        if (healthUI != null) return;

        GameObject uiObj = Instantiate(
            CommonReferent.Instance.hpSliderUi,
            transform.position,
            Quaternion.identity
        );

        uiObj.transform.SetParent(CommonReferent.Instance.canvasHp.transform, false);
        uiObj.transform.localScale = Vector3.one;

        healthUI = uiObj.GetComponent<EnemyHealthUI>();
        healthUI.SetTarget(gameObject);
    }
   
    
        private bool hasSpawnedEnemy = false;

    private void OnEnable()
    {
        spriteRenderer.color = Color.white;
        transform.localPosition = originalPos;
        DestructibleTracker.Instance?.Register(this);

        currentHealth = maxHealth;
        hasSpawnedEnemy = false;                    // Reset mỗi lần bật lại

        if (healthUI == null)
            CreateHealthUI();

        healthUI?.UpdateHealth(currentHealth);

        if (alwaysShowHP)
            healthUI?.ShowUI();
        else
            healthUI?.HideUI();
    }

    private void HandleDestruction()
    {
        // VFX phá
        if (CommonReferent.Instance.destructionVFXPrefab != null)
        {
            ObjectPooler.Instance.Get(
                "BreakVFX",
                CommonReferent.Instance.destructionVFXPrefab,
                transform.position,
                Quaternion.identity
            );
        }

        // DROP VÀNG + GEM (giữ nguyên code của bạn)

        // DROP VÀNG
        int totalGold = Random.Range(minGold, maxGold + 1);
        for (int i = 0; i < totalGold; i++)
        {
            Vector3 offset = Random.insideUnitCircle * 0.5f;
            ObjectPooler.Instance.Get("Gold", CommonReferent.Instance.goldPrefab, transform.position + offset, Quaternion.identity);
        }

        // DROP GEM
        if (Random.value < gemDropChance)
        {
            int totalGem = Random.Range(minGem, maxGem + 1);
            for (int i = 0; i < totalGem; i++)
            {
                Vector3 offset = Random.insideUnitCircle * 0.5f;
                ObjectPooler.Instance.Get("Gem", CommonReferent.Instance.gemPrefab, transform.position + offset, Quaternion.identity);
            }
        }

        // ==================== SPAWN ENEMY ====================
        if (spawnEnemyOnBreak && !hasSpawnedEnemy && Random.value <= enemySpawnChance)
        {
            hasSpawnedEnemy = true;
            SpawnEnemyLogic();
        }

        healthUI?.HideUI();
        QuestManager.Instance.ReportProgress("NV2", nameOBJ, 1);

        gameObject.SetActive(false);

        // Hồi sinh
        DestructibleTracker.Instance.StartCoroutine(RespawnAfterDelay());
    }

    private void SpawnEnemyLogic()
    {
        if (enemyPrefabs.Length == 0) return;

        GameObject pick = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        if (pick == null) return;

        // Spawn enemy
        GameObject enemyObj = Instantiate(pick, transform.position, Quaternion.identity,
            CommonReferent.Instance.enemyRoot.transform);

        var ai = enemyObj.GetComponent<EnemyAI>();
        if (ai == null) return;

        // Tạo Health UI mới
        GameObject uiObj = Instantiate(
            CommonReferent.Instance.hpSliderUi,
            transform.position,
            Quaternion.identity
        );

        uiObj.transform.SetParent(CommonReferent.Instance.canvasHp.transform, false);
        uiObj.transform.localScale = Vector3.one;

        EnemyHealthUI newHealthUI = uiObj.GetComponent<EnemyHealthUI>();
        if (newHealthUI == null) return;

        // GÁN ĐÚNG CÁCH
        ai.EnemyHealthUI = newHealthUI;
        ai.AssignHealthUI(newHealthUI);   // hàm này đã có ForceSetTarget

        // Reset enemy (quan trọng để set HP, alwaysShowHP, v.v.)
        ai.ResetEnemy();
    }
   
    private string nameOBJ = "Barrier";
    [SerializeField] private string objectiveID = "Barrier";

    public string ObjectiveID => objectiveID;
}
