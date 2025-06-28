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

    private GameObject targetEnemy;
    private float hideTimer;
    private int maxHealth;

    public void SetTarget(GameObject enemy)
    {
        targetEnemy = enemy;

        var ai = enemy.GetComponent<EnemyAI>();
        maxHealth = ai.MaxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;

        if (nameAndLevelText != null)
        {
            nameAndLevelText.text = $"{ai.EnemyName} (Lv {ai.EnemyLevel})";
        }

        if (hpText != null)
        {
            hpText.text = $"{maxHealth}/{maxHealth}";
        }

        gameObject.SetActive(false); 
    }

    public void UpdateHealth(int currentHealth)
    {
        healthSlider.value = currentHealth;

        if (hpText != null)
        {
            hpText.text = $"{currentHealth}/{maxHealth}";
        }

        gameObject.SetActive(true); 
        hideTimer = hideDelay;
    }

    private void LateUpdate()
    {
        if (targetEnemy == null) return;
        
        Vector3 worldPos = targetEnemy.transform.position + offset;
        if (Camera.main != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            transform.position = screenPos;
        }
        
        if (gameObject.activeSelf)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}