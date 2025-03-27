using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public int damage = 10;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Đã hồi {amount} máu. Máu hiện tại: {currentHealth}/{maxHealth}");
    }

    public void ModifyMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // Giới hạn máu không vượt quá maxHealth
        Debug.Log($"Máu tối đa đã thay đổi. Máu hiện tại: {currentHealth}/{maxHealth}");
    }
}