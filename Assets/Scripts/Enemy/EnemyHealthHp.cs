// ================= EnemyHealthUI.cs =================
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI nameAndLevelText;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Position")]
    [SerializeField] private Vector3 offset = new Vector3(0, 1f, 0);

    [Header("Auto Hide")]
    [SerializeField] private float hideDelay = 2f;

    [Header("Colors")]
    [SerializeField] private Color enemyHpColor = Color.red;
   
    private Camera mainCamera;
    private float hideTimer;
    private int maxHealth;
    private bool autoHide;

    private TargetInfo currentTarget;
    public TargetInfo CurrentTarget => currentTarget;   

    public class TargetInfo
    {
        public GameObject target;
        public bool isEnemy;
        public bool isAlly;
        public int maxHealth;
        public string displayName;
        public Color sliderColor;
    }

    #region Unity Lifecycle

    private void Awake()
    {
        mainCamera = Camera.main;
        HideUI();
    }

    private void OnDestroy()
    {
        currentTarget = default;
    }
    
    private bool IsTargetVisible()
    {
        if (currentTarget == null || currentTarget.target == null || mainCamera == null)
            return false;

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(currentTarget.target.transform.position);

        return viewportPos.z > 0 &&
               viewportPos.x > 0 && viewportPos.x < 1 &&
               viewportPos.y > 0 && viewportPos.y < 1;
    }

    private void LateUpdate()
    {
        if (!IsTargetVisible())
        {
            HideUI();
            return;
        }
      
        if (gameObject == null || !gameObject.activeInHierarchy) return;
        if (currentTarget == null || currentTarget.target == null)
        {
            Destroy(gameObject);
            return;
        }
        
        // UI hoặc script đã bị destroy → thoát
        if (gameObject == null) return;

        // Không có target hoặc target đã bị destroy
        if (currentTarget == null || !currentTarget.target)
        {
            Destroy(gameObject);
            return;
        }

        // Player chết → xoá UI
        if (PlayerController.Instance != null &&
            PlayerController.Instance.IsPlayerDie())
        {
            Destroy(gameObject);
            return;
        }

        // Enemy chết → xoá UI
        if (currentTarget.isEnemy || currentTarget.isAlly)
        {
            // TUYỆT ĐỐI không gọi TryGetComponent nếu target có thể chết
            if (!currentTarget.target)
            {
                Destroy(gameObject);
                return;
            }

            if (currentTarget.target.TryGetComponent(out EnemyAI enemy) && enemy.IsDead)
            {
                Destroy(gameObject);
                return;
            }
            
            if (currentTarget.target.TryGetComponent(out AllyBaseAI ally) && ally.IsDead)
            {
                Destroy(gameObject);
                return;
            }
        }

        // Follow world position
        if (mainCamera != null)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(
                currentTarget.target.transform.position + offset);

            transform.position = screenPos;
        }

        // Auto hide (enemy only)
        if (autoHide && gameObject.activeSelf)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f)
                HideUI();
        }
    }


    #endregion

    #region Public API

  

    public void SetTarget(GameObject target)
{
    if (!this) return;
    
    // Nếu đã set đúng target này rồi → chỉ refresh HP
    if (currentTarget != null && currentTarget.target == target)
    {
        RefreshHealthFromTarget();
        return;
    }
    
    HideUI();
    hideTimer = hideDelay;
    currentTarget = default;

    if (target == null) return;

    TargetInfo info = new TargetInfo { target = target };

    if (target.TryGetComponent(out EnemyAI enemy))
    {
        info.isEnemy = true;
        info.maxHealth = enemy.MaxHealth;
        info.displayName = $"{enemy.EnemyName} (Lv {enemy.EnemyLevel})";
        info.sliderColor = enemyHpColor;
        autoHide = false;
    }
    else if (target.TryGetComponent(out AllyStats ally))
    {
        info.isAlly = true;
        info.maxHealth = ally.MaxHP;
        info.displayName = $"{ally.HeroName}";
        info.sliderColor = enemyHpColor;
        autoHide = false;
    }
    else if (target.TryGetComponent(out DestructibleObject destruct))
    {
        info.isEnemy = true;  // hoặc false tùy bạn
        info.maxHealth = destruct.MaxHealth;
        info.displayName = destruct.displayName;
        info.sliderColor = Color.yellow;
        autoHide = false;           // ← quan trọng
    }
    else
    {
        info.isEnemy = true;
        info.maxHealth = 1;
        info.displayName = "Unknown";
        info.sliderColor = enemyHpColor;
        autoHide = true;
    }

    currentTarget = info;
    maxHealth = info.maxHealth;

    healthSlider.maxValue = maxHealth;
    healthSlider.value = maxHealth;

    if (nameAndLevelText != null)
        nameAndLevelText.text = info.displayName;

    if (hpText != null)
        hpText.text = $"{maxHealth}/{maxHealth}";

    SetSliderColor(info.sliderColor);

    gameObject.SetActive(true);
}

    private void RefreshHealthFromTarget()
    {
        if (currentTarget == null || currentTarget.target == null) return;

        int currentHp = maxHealth;

        if (currentTarget.target.TryGetComponent(out EnemyAI enemy))
        {
            currentHp = enemy.CurrentHealth;
        }
        else if (currentTarget.target.TryGetComponent(out AllyStats ally))
        {
            currentHp = ally.CurrentHP;
        }
        else if (currentTarget.target.TryGetComponent(out DestructibleObject destruct))
        {
            currentHp = destruct.CurrentHealth;   // đảm bảo property này public và getter đúng
        }

        UpdateHealth(currentHp);
    }
    public void UpdateHealth(int currentHealth)
    {
        if (!this) return;
        if (currentTarget.target == null) return;

        if (PlayerController.Instance != null && PlayerController.Instance.IsPlayerDie())
        {
            HideUI();
            return;
        }

        healthSlider.value = currentHealth;
        if (hpText != null)
            hpText.text = $"{currentHealth}/{maxHealth}";

        // Luôn hiển thị khi đang cập nhật HP
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        // Không cần reset hideTimer nữa vì autoHide = false
    }

    #endregion

    #region Helpers

    private void SetSliderColor(Color color)
    {
        if (healthSlider.fillRect == null) return;

        Image fillImage = healthSlider.fillRect.GetComponent<Image>();
        if (fillImage != null)
            fillImage.color = color;
    }

    #endregion

    public void ShowUI()
    {
        if (!this || gameObject == null) return;
        gameObject.SetActive(true);
        hideTimer = hideDelay;   // reset timer nếu có auto hide
    }

    public void HideUI()
    {
        if (!this) return;
        if (gameObject != null && gameObject.activeSelf)
            gameObject.SetActive(false);
    }
    // Thêm vào #region Public API
    public void ForceSetTarget(GameObject target)
    {
        // Destroy tất cả UI khác đang trỏ vào cùng target này
        var allUIs = FindObjectsOfType<EnemyHealthUI>(true);
        foreach (var ui in allUIs)
        {
            if (ui != this && ui.currentTarget != null && ui.currentTarget.target == target)
            {
                Destroy(ui.gameObject);
            }
        }

        SetTarget(target);
    }
}
