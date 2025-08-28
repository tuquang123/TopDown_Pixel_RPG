using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HealingZone : MonoBehaviour
{
    [Header("Heal Config")]
    public int healHealthAmount = 20;   // hồi bao nhiêu máu mỗi tick
    public int healManaAmount = 10;     // hồi bao nhiêu mana mỗi tick
    public float healInterval = 1f;     // thời gian giữa mỗi lần hồi (giây)

    private bool playerInside = false;
    private Coroutine healRoutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            healRoutine = StartCoroutine(HealPlayer(other.GetComponent<PlayerStats>()));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            if (healRoutine != null) StopCoroutine(healRoutine);
        }
    }

    private IEnumerator HealPlayer(PlayerStats playerStats)
    {
        while (playerInside && playerStats != null && !playerStats.isDead)
        {
            // Heal máu
            int beforeHP = (int)playerStats.GetCurrentHealth();
            playerStats.Heal(healHealthAmount);
            int afterHP = (int)playerStats.GetCurrentHealth();

            int totalHealed = afterHP - beforeHP;
            if (totalHealed > 0)
            {
                FloatingTextSpawner.Instance.SpawnText(
                    $"+{totalHealed}",
                    playerStats.transform.position + Vector3.up,
                    new Color(0.3f, 1f, 0.3f) // xanh lá
                );
            }

            // Heal mana
            int beforeMP = playerStats.currentMana;
            playerStats.RestoreMana(healManaAmount);
            int afterMP = playerStats.currentMana;

            int totalManaRestored = afterMP - beforeMP;
            if (totalManaRestored > 0)
            {
                FloatingTextSpawner.Instance.SpawnText(
                    $"+{totalManaRestored} MP",
                    playerStats.transform.position + Vector3.up * 1.2f,
                    new Color(0.3f, 0.6f, 1f) // xanh dương
                );
            }

            yield return new WaitForSeconds(healInterval);
        }
    }
}
