using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HealingZone : MonoBehaviour
{
    [Header("Heal Config (percent of max stats)")]
    [Range(0f, 1f)] public float healHealthPercent = 0.05f; // 5% max HP mỗi tick
    [Range(0f, 1f)] public float healManaPercent = 0.03f;   // 3% max MP mỗi tick
    public float healInterval = 1f;                         // thời gian giữa mỗi lần hồi (giây)

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
            // --- Heal máu theo % ---
            int healHP = Mathf.RoundToInt(playerStats.maxHealth.Value  * healHealthPercent);
            int beforeHP = (int)playerStats.GetCurrentHealth();
            playerStats.Heal(healHP);
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

            // --- Heal mana theo % ---
            int healMP = Mathf.RoundToInt(playerStats.maxMana.Value  * healManaPercent);
            int beforeMP = playerStats.currentMana;
            playerStats.RestoreMana(healMP);
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
