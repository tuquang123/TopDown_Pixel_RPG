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

    private GameObject targetEnemy;
    private float hideTimer;
    private int maxHealth;

    public void HideUI()
    {
        gameObject.SetActive(false);
    }
    
    private void SetSliderColor(Color color)
    {
        if (healthSlider.fillRect != null)
        {
            var fillImage = healthSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = color;
            }
        }
    }

    public void SetTarget(GameObject target)
    {
        targetEnemy = target;

        if (target.TryGetComponent(out EnemyAI enemy))
        {
            maxHealth = enemy.MaxHealth;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
            
            if (nameAndLevelText != null)
                nameAndLevelText.text = $"{enemy.EnemyName} (Lv {enemy.EnemyLevel})";

            if (hpText != null)
                hpText.text = $"{maxHealth}/{maxHealth}";
            
            SetSliderColor(enemyHpColor);

            autoHide = true;
        }
        else if (target.TryGetComponent(out AllyStats ally))
        {
            maxHealth = ally.MaxHP;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;

            if (nameAndLevelText != null)
                nameAndLevelText.text = $"{ally.HeroName} (SP)";

            if (hpText != null)
                hpText.text = $"{maxHealth}/{maxHealth}";
            
            SetSliderColor(allyHpColor);

            autoHide = false;
        }

        else
        {
            Debug.LogWarning("SetTarget(): Không tìm thấy EnemyAI hoặc AllyStats trên target.");
        }

        //gameObject.SetActive(false);
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

        gameObject.SetActive(true);
        hideTimer = hideDelay;
    }


    private void LateUpdate()
    {
        if (targetEnemy == null) return;

        if (targetEnemy.TryGetComponent(out EnemyAI enemy) && enemy.IsDead)
        {
            HideUI();
            return;
        }

        if (PlayerController.Instance != null && PlayerController.Instance.IsPlayerDie())
        {
            HideUI();
            return;
        }

        Vector3 worldPos = targetEnemy.transform.position + offset;
        if (Camera.main != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            transform.position = screenPos;
        }

        if (autoHide && gameObject.activeSelf)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0)
                gameObject.SetActive(false);
        }
    }

}