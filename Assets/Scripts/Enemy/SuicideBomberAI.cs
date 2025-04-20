using Cysharp.Threading.Tasks;
using UnityEngine;

public class SuicideBomberAI : EnemyAI
{
    [SerializeField] private float explosionDelay = 3f;
    [SerializeField] private int explosionDamage = 30;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private float moveToTargetSpeed = 4f;

    private Vector2 cachedTargetPosition;
    private bool isExploding = false;

    protected override async void HandleAttackState()
    {
        if (isExploding) return;

        // Khóa vị trí player tại thời điểm bắt đầu tấn công
        cachedTargetPosition = player.position;
        isExploding = true;

        await MoveToTargetPosition();

        // Đứng yên tại vị trí đã đến và đợi 3 giây rồi phát nổ
        await UniTask.Delay(System.TimeSpan.FromSeconds(explosionDelay));

        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        if (player.TryGetComponent(out PlayerHealth playerHealth))
        {
            playerHealth.TakeDamage(explosionDamage);
        }

        SetState(EnemyState.Dead);
    }

    private async UniTask MoveToTargetPosition()
    {
        while (Vector2.Distance(transform.position, cachedTargetPosition) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, cachedTargetPosition, moveToTargetSpeed * Time.deltaTime);
            transform.rotation = cachedTargetPosition.x > transform.position.x
                ? Quaternion.Euler(0, 0, 0)
                : Quaternion.Euler(0, 180, 0);

            await UniTask.Yield();
        }
    }
}