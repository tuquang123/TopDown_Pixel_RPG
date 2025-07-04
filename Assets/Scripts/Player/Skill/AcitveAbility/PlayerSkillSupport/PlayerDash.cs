﻿using System.Collections;
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

    public void PerformDash(SkillData skill , PlayerStats playerStats)
    {
        if (isDashing) return;
        
        Transform target = playerMovement.GetTargetEnemy();
        if (target == null) return;

        Vector2 directionToTarget = (target.position - transform.position).normalized;
        StartCoroutine(DashCoroutine(skill, directionToTarget, playerStats));
    }

    private IEnumerator DashCoroutine(SkillData skill, Vector2 direction , PlayerStats playerStats)
    {
        isDashing = true;
        float startTime = Time.time;
        
        if (playerMovement != null)
        {
            playerMovement.RotateCharacter(direction.x);
        }
        
        // ✅ Spawn dash VFX (ví dụ: khói hoặc hiệu ứng chớp nhoáng)
        var prefab = skill.GetPrefabAtLevel(playerStats.GetSkillLevel(skill.skillID));
        var vfx = Instantiate(prefab, transform.position, Quaternion.identity, transform);
        
        AudioManager.Instance.PlaySFX("Dash");

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