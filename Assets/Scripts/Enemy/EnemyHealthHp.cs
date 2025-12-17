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
    [SerializeField] private Color allyHpColor = Color.green;

    private Camera mainCamera;
    private float hideTimer;
    private int maxHealth;
    private bool autoHide;

    private TargetInfo currentTarget;

    private struct TargetInfo
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

    private void LateUpdate()
    {
        if (!this) return;
        if (currentTarget.target == null) return;

        if (PlayerController.Instance != null &&
            PlayerController.Instance.IsPlayerDie())
        {
            HideUI();
            return;
        }

        // Enemy chết → hide
        if (currentTarget.isEnemy &&
            currentTarget.target.TryGetComponent(out EnemyAI enemy) &&
            enemy.IsDead)
        {
            HideUI();
            return;
        }

        // Follow world position
        if (mainCamera != null)
        {
            Vector3 screenPos =
                mainCamera.WorldToScreenPoint(
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

    public void HideUI()
    {
        if (!this) return;
        if (gameObject != null && gameObject.activeSelf)
            gameObject.SetActive(false);
    }

    public void SetTarget(GameObject target)
    {
        if (!this) return;

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
            autoHide = true;
        }
        else if (target.TryGetComponent(out AllyStats ally))
        {
            info.isAlly = true;
            info.maxHealth = ally.MaxHP;
            info.displayName = $"{ally.HeroName}";
            info.sliderColor = allyHpColor;
            autoHide = false;
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

        if (!autoHide)
            gameObject.SetActive(true);
    }

    public void UpdateHealth(int currentHealth)
    {
        // ===== FIX CỐT LÕI MissingReferenceException =====
        if (!this) return;
        if (currentTarget.target == null) return;

        if (PlayerController.Instance != null &&
            PlayerController.Instance.IsPlayerDie())
        {
            HideUI();
            return;
        }

        healthSlider.value = currentHealth;

        if (hpText != null)
            hpText.text = $"{currentHealth}/{maxHealth}";

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        hideTimer = hideDelay;
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
}
