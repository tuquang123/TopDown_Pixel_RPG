// ================= EnemyHealthUI.cs =================
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Vector3 offset = new Vector3(0, 1f, 0);
    [SerializeField] private float hideDelay = 2f;

    [Header("Text UI")]
    [SerializeField] private TextMeshProUGUI nameAndLevelText;
    [SerializeField] private TextMeshProUGUI hpText; 

    [SerializeField] private Color enemyHpColor = Color.red;
    [SerializeField] private Color allyHpColor = Color.green;

    private bool autoHide = true;
    private float hideTimer;
    private int maxHealth;

    private Camera mainCamera;
    private TargetInfo currentTarget;

    private struct TargetInfo
    {
        public GameObject target;
        public bool isEnemy;
        public bool isAlly;
        public bool isDead;
        public int maxHealth;
        public string displayName;
        public Color sliderColor;
    }

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void HideUI() => gameObject.SetActive(false);

    private void SetSliderColor(Color color)
    {
        if (healthSlider.fillRect != null)
        {
            var fillImage = healthSlider.fillRect.GetComponent<Image>();
            if (fillImage != null) fillImage.color = color;
        }
    }

    public void SetTarget(GameObject target)
    {
        // Reset UI state
        HideUI();
        hideTimer = hideDelay;
        currentTarget = new TargetInfo { target = null };

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
            info.displayName = $"{ally.HeroName} (SP)";
            info.sliderColor = allyHpColor;
            autoHide = false;
        }
        else
        {
            // fallback
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

        if (nameAndLevelText != null) nameAndLevelText.text = info.displayName;
        if (hpText != null) hpText.text = $"{maxHealth}/{maxHealth}";

        SetSliderColor(info.sliderColor);

        gameObject.SetActive(!autoHide); // Ally always visible, Enemy hidden until damaged
    }

    public void UpdateHealth(int currentHealth)
    {
        if (PlayerController.Instance != null && PlayerController.Instance.IsPlayerDie())
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

    private void LateUpdate()
    {
        if (currentTarget.target == null) return;

        // Check if target is dead
        if (currentTarget.isEnemy && currentTarget.target.TryGetComponent(out EnemyAI enemy) && enemy.IsDead)
        {
            HideUI();
            return;
        }

        if (PlayerController.Instance != null && PlayerController.Instance.IsPlayerDie())
        {
            HideUI();
            return;
        }

        // Update position
        if (mainCamera != null)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(currentTarget.target.transform.position + offset);
            transform.position = screenPos;
        }

        // Auto hide logic
        if (autoHide)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f && gameObject.activeSelf)
                HideUI();
        }
    }
}
