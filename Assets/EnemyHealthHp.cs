using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Vector3 offset = new Vector3(0, 1f, 0);
    [SerializeField] private float hideDelay = 2f;

    private GameObject targetEnemy;
    private float hideTimer;

    public void SetTarget(GameObject enemy)
    {
        targetEnemy = enemy;
        healthSlider.maxValue = enemy.GetComponent<EnemyAI>().Maxhealth;
        healthSlider.value = enemy.GetComponent<EnemyAI>().Maxhealth;
        gameObject.SetActive(false); // ẩn ban đầu
    }

    public void UpdateHealth(int currentHealth)
    {
        healthSlider.value = currentHealth;
        gameObject.SetActive(true);  // nhận damage thì bật lên
        hideTimer = hideDelay;       // reset timer ẩn
    }

    private void LateUpdate()
    {
        if (targetEnemy == null) return;

        // Update vị trí
        Vector3 worldPos = targetEnemy.transform.position + offset;
        if (Camera.main != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            transform.position = screenPos;
        }

        // Đếm ngược timer ẩn
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