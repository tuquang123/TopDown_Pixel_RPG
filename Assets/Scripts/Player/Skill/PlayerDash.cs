using System.Collections;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    public float dashForce = 10f;
    public float dashDuration = 0.2f;
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

        Vector2 inputDir = playerMovement.GetMoveInput(); // ví dụ trả về Vector2(-1,0) hoặc (1,0)
        if (inputDir == Vector2.zero) return; // không dash nếu không có hướng

        StartCoroutine(DashCoroutine(skill, inputDir.normalized));
    }

    private IEnumerator DashCoroutine(SkillData skill, Vector2 direction)
    {
        isDashing = true;
        float startTime = Time.time;
        GetComponent<PlayerStats>().currentMana -= skill.manaCost;

        while (Time.time < startTime + dashDuration)
        {
            rb.linearVelocity = direction * dashForce;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        isDashing = false;
    }
}