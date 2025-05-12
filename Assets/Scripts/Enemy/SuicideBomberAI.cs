using Cysharp.Threading.Tasks;
using UnityEngine;

public class SuicideBomberAI : EnemyAI
{
    [SerializeField] private float explosionDelay = 3f;
    [SerializeField] private int explosionDamage = 30;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private float moveToTargetSpeed = 4f;
    [SerializeField] private float explosionRange = 2f;
    [SerializeField] private float flashDuration = 0.1f;  // Thời gian một lần nháy
    [SerializeField] private int flashCount = 5; // Số lần nháy
    [SerializeField]private SpriteRenderer spriteRenderer;

    private Vector2 cachedTargetPosition;
    private bool isExploding = false;
    private Color originalColor;

    protected override void Start()
    {
        base.Start();
        //spriteRenderer = GetComponentInChildren<SpriteRenderer>();  // Lấy SpriteRenderer của enemy
        originalColor = spriteRenderer.color;  // Lưu lại màu gốc của sprite
    }

    protected override void AttackPlayer()
    {
        if (player.TryGetComponent(out PlayerStats playerStats))
        {
            if(playerStats.isDead) return;
        }
        if (isExploding || player == null) return;

        cachedTargetPosition = player.position;
        isExploding = true;

        anim.SetBool(MoveBool, true);
        _ = StartExplosionSequence();
    }

    private async UniTaskVoid StartExplosionSequence()
    {
        await MoveToTargetPosition();

        anim.SetBool(MoveBool, false);
        await UniTask.Delay(System.TimeSpan.FromSeconds(explosionDelay));

        // Bắt đầu hiệu ứng nháy
        _ = FlashEffect();
        
        // Đợi sau khi hiệu ứng nháy, rồi tiến hành nổ
        await UniTask.Delay(System.TimeSpan.FromSeconds(flashDuration * flashCount));  // Delay thêm để nháy trước khi nổ

        Explode();
    }

    private async UniTask MoveToTargetPosition()
    {
        while (Vector2.Distance(transform.position, cachedTargetPosition) > 0.1f && !isDead)
        {
            transform.position = Vector2.MoveTowards(transform.position, cachedTargetPosition, moveToTargetSpeed * Time.deltaTime);

            transform.rotation = cachedTargetPosition.x > transform.position.x
                ? Quaternion.Euler(0, 0, 0)
                : Quaternion.Euler(0, 180, 0);

            await UniTask.Yield();
        }
    }

    private void Explode()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        if (player != null && Vector2.Distance(player.position, transform.position) <= explosionRange)
        {
            if (player.TryGetComponent(out PlayerStats playerHealth))
            {
                playerHealth.TakeDamage(explosionDamage);
            }
        }

        base.Die();
    }

    private async UniTask FlashEffect()
    {
        // Flash effect: Thay đổi màu sắc sprite trong thời gian ngắn để tạo hiệu ứng nháy
        for (int i = 0; i < flashCount; i++)
        {
            // Thay đổi màu của sprite thành màu sáng
            spriteRenderer.color = Color.red;  // Hoặc có thể thay đổi màu khác nếu muốn
            await UniTask.Delay(System.TimeSpan.FromSeconds(flashDuration));

            // Quay lại màu gốc
            spriteRenderer.color = originalColor;
            await UniTask.Delay(System.TimeSpan.FromSeconds(flashDuration));
        }
    }
}
