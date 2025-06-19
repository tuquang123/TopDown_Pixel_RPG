using System.Collections;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    public float dashForce = 10f;
    public float dashDuration = 0.2f;
    public float timeScale = 0.8f;
    
    private bool isDashing = false;
    private Rigidbody2D rb;
    private PlayerController playerMovement; // class đọc input joystick
    public bool IsDashing => isDashing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerController>();
    }

    public void PerformDash(SkillData skill)
    {
        if (isDashing) return;
        
        Transform target = playerMovement.GetTargetEnemy();
        if (target == null) return;

        Vector2 directionToTarget = (target.position - transform.position).normalized;
        StartCoroutine(DashCoroutine(skill, directionToTarget));
    }

    private IEnumerator DashCoroutine(SkillData skill, Vector2 direction)
    {
        isDashing = true;
        float startTime = Time.time;
        
        if (playerMovement != null)
        {
            playerMovement.RotateCharacter(direction.x);
        }
        
        // ✅ Spawn dash VFX (ví dụ: khói hoặc hiệu ứng chớp nhoáng)
        var vfx = Instantiate(skill.prefab, transform.position, Quaternion.identity, transform);

        // ✅ Slow motion nhẹ
        float originalTimeScale = Time.timeScale;
        Time.timeScale = timeScale; // giảm thời gian 30% cho cảm giác slow motion

        while (Time.time < startTime + dashDuration)
        {
            rb.linearVelocity = direction * dashForce;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        isDashing = false;

        // ✅ Khôi phục thời gian
        Time.timeScale = originalTimeScale;

        // ✅ Hủy VFX
        if (vfx != null) Destroy(vfx);
    }
}