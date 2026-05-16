using System.Collections;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    public float dashForce = 10f;
    public float dashDuration = 0.2f;
    public float timeScale = 0.8f;
    
    private bool isDashing = false;
    private Rigidbody2D rb;
    private PlayerController playerMovement; 
    public bool IsDashing => isDashing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerController>();
    }

    public void PerformDash(SkillData skill, PlayerStats playerStats)
    {
        if (isDashing) return;

        Transform target = playerMovement.GetTargetEnemy();
        Vector2 direction;

        if (target != null)
        {
            direction = (target.position - transform.position).normalized;
        }
        else
        {
            Vector2 inputDir = playerMovement.MoveInput;
            if (inputDir.sqrMagnitude < 0.01f) 
                return;

            direction = inputDir.normalized;
        }

        StartCoroutine(DashCoroutine(skill, direction, playerStats));
    }



    private IEnumerator DashCoroutine(SkillData skill, Vector2 direction , PlayerStats playerStats)
    {
        isDashing = true;
        float startTime = Time.time;
        
        if (playerMovement != null)
        {
            playerMovement.RotateCharacter(direction.x);
        }
        
        var prefab = skill.GetPrefabAtLevel(playerStats.GetSkillLevel(skill.skillID));
        var vfx = Instantiate(prefab, transform.position, Quaternion.identity, transform);
        
        AudioManager.Instance.PlaySFX("Dash");
        
        float originalTimeScale = Time.timeScale;
        Time.timeScale = timeScale; 

        while (Time.time < startTime + dashDuration)
        {
            rb.linearVelocity = direction * dashForce;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        isDashing = false;
        
        Time.timeScale = originalTimeScale;
        
        if (vfx != null) Destroy(vfx);
    }
}