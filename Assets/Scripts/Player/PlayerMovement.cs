using UnityEngine;

public class PlayerMovement
{
    private readonly PlayerController owner;

    public PlayerMovement(PlayerController owner)
    {
        this.owner = owner;
    }

    public Vector2 GetMoveInput()
    {
        float joyX = UltimateJoystick.GetHorizontalAxis("Move");
        float joyY = UltimateJoystick.GetVerticalAxis("Move");

        float keyX = Input.GetKey(KeyCode.A) ? -1f : Input.GetKey(KeyCode.D) ? 1f : 0f;
        float keyY = Input.GetKey(KeyCode.S) ? -1f : Input.GetKey(KeyCode.W) ? 1f : 0f;

        Vector2 combined = new Vector2(joyX + keyX, joyY + keyY);
        return combined.magnitude > 1f ? combined.normalized : combined;
    }

    public void MovePlayer()
    {
        ApplyMoveVelocity();
        RotateCharacter(owner.MoveInput.x);
    }

    public void ApplyMoveVelocity()
    {
        if (owner.Body == null)
            return;

        owner.Body.linearVelocity = owner.MoveInput.normalized * owner.GetHeavyWeaponMoveSpeed();
    }

    public void MoveToTarget(Transform target, System.Action onFacing = null)
    {
        if (target == null || owner.Stats == null || owner.Stats.isDead || owner.Body == null || owner.Animator == null)
            return;

        Vector2 playerPos = owner.transform.position;
        Vector2 targetPos = target.position;
        float distance = Vector2.Distance(playerPos, targetPos);
        float moveSpeed = owner.GetHeavyWeaponMoveSpeed();
        float currentAttackRange = owner.GetCurrentAttackRange();
        float closeCombatAttackBuffer = 0.2f;

        if (owner.typeWeapon == WeaponCategory.Bow)
        {
            if (distance > currentAttackRange)
            {
                Vector2 dir = (targetPos - playerPos).normalized;
                owner.Body.linearVelocity = dir * moveSpeed;
                owner.Animator.SetBool(PlayerController.MoveBool, true);
            }
            else
            {
                Stop();
                RotateToTarget(target);
                owner.TryAttack();
            }

            return;
        }

        if (owner.typeWeapon == WeaponCategory.Ranged)
        {
            if (distance > currentAttackRange)
            {
                Vector2 dir = (targetPos - playerPos).normalized;
                owner.Body.linearVelocity = dir * moveSpeed;
                RotateCharacter(dir.x);
                owner.Animator.SetBool(PlayerController.MoveBool, true);
            }
            else
            {
                Stop();
                RotateCharacter(targetPos.x - playerPos.x);
                onFacing?.Invoke();
                owner.TryAttack();
            }

            return;
        }

        // Melee: cho phép đánh khi đã gần nhau kể cả đang lệch chéo nhẹ.
        if (distance <= currentAttackRange + closeCombatAttackBuffer)
        {
            Stop();
            RotateCharacter(targetPos.x - playerPos.x);
            onFacing?.Invoke();
            owner.TryAttack();
            return;
        }

        float yDiff = Mathf.Abs(playerPos.y - targetPos.y);
        float xDiff = Mathf.Abs(playerPos.x - targetPos.x);

        if (yDiff > 0.1f)
        {
            Vector2 dirY = new Vector2(0f, targetPos.y - playerPos.y).normalized;
            owner.Body.linearVelocity = dirY * moveSpeed;
            owner.Animator.SetBool(PlayerController.MoveBool, true);
        }
        else if (xDiff > currentAttackRange * 0.8f)
        {
            Vector2 dirX = new Vector2(targetPos.x - playerPos.x, 0f).normalized;
            owner.Body.linearVelocity = dirX * moveSpeed;
            RotateCharacter(dirX.x);
            owner.Animator.SetBool(PlayerController.MoveBool, true);
        }
        else
        {
            Stop();
            RotateCharacter(targetPos.x - playerPos.x);
            onFacing?.Invoke();
            owner.TryAttack();
        }
    }

    public void RotateCharacter(float direction)
    {
        if (owner.IsDashing || owner.IsAttacking)
            return;

        if (direction < 0f)
            owner.transform.localScale = Vector3.one;
        else if (direction > 0f)
            owner.transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    public void RotateToTarget(Transform target)
    {
        if (target == null)
            return;

        RotateCharacter(target.position.x - owner.transform.position.x);
    }

    private void Stop()
    {
        owner.Body.linearVelocity = Vector2.zero;
        owner.Animator.SetBool(PlayerController.MoveBool, false);
    }
}
