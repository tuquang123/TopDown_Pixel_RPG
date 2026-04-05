using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HealingZone : MonoBehaviour
{
    [Header("Heal Config (percent of max stats)")]
    [Range(0f, 1f)] public float healHealthPercent = 0.05f;
    [Range(0f, 1f)] public float healManaPercent = 0.03f;
    public float healInterval = 1f;

    [Header("Zone Config")]
    [SerializeField] private bool isHealingArena = true; // ✅ check quest zone

    private bool playerInside = false;
    private Coroutine healRoutine;
    private PlayerStats currentPlayer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            currentPlayer = other.GetComponent<PlayerStats>();

            // ❌ BỎ cái này
            // currentPlayer.OnHealed += OnPlayerHealed;

            // ✅ HOÀN THÀNH QUEST NGAY LẬP TỨC
            if (isHealingArena)
            {
                QuestManager.Instance.ReportProgress("NV3", "EnterZone", 1);
            }

            healRoutine = StartCoroutine(HealPlayer(currentPlayer));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;

            if (healRoutine != null)
                StopCoroutine(healRoutine);

            // ✅ bỏ đăng ký event
            if (currentPlayer != null)
            currentPlayer = null;
        }
    }

    private IEnumerator HealPlayer(PlayerStats playerStats)
    {
        while (playerInside && playerStats != null && !playerStats.isDead)
        {
            // --- Heal HP ---
            int healHP = Mathf.RoundToInt(playerStats.maxHealth.Value * healHealthPercent);

            float beforeHP = playerStats.GetCurrentHealth();
            playerStats.Heal(healHP);
            float afterHP = playerStats.GetCurrentHealth();

            int totalHealed = Mathf.RoundToInt(afterHP - beforeHP);

            if (totalHealed > 0)
            {
                FloatingTextSpawner.Instance.SpawnText(
                    $"+{totalHealed}",
                    playerStats.transform.position + Vector3.up,
                    new Color(0.3f, 1f, 0.3f)
                );
            }

            // --- Heal MP ---
            int healMP = Mathf.RoundToInt(playerStats.maxMana.Value * healManaPercent);

            int beforeMP = playerStats.currentMana;
            playerStats.RestoreMana(healMP);
            int afterMP = playerStats.currentMana;

            int totalMana = afterMP - beforeMP;

            if (totalMana > 0)
            {
                FloatingTextSpawner.Instance.SpawnText(
                    $"+{totalMana} MP",
                    playerStats.transform.position + Vector3.up * 1.2f,
                    new Color(0.3f, 0.6f, 1f)
                );
            }

            yield return new WaitForSeconds(healInterval);
        }
    }

   
}