using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private GameObject bossUIRoot;

    public void SetMaxHealth(int maxHealth)
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
        bossUIRoot.SetActive(true);
    }

    public void UpdateHealth(int currentHealth)
    {
        healthSlider.value = currentHealth;
    }

    public void Hide()
    {
        bossUIRoot.SetActive(false);
    }
}