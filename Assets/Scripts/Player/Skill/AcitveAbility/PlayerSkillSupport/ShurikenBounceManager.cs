using UnityEngine;
using System.Collections;

public class ShurikenBounceManager : MonoBehaviour
{
    public GameObject shurikenPrefab;
    public Transform player;
    public float shootInterval = 5f;
    public float shurikenSpeed = 5f; // Tốc độ phi tiêu

    void Start()
    {
        StartCoroutine(ShootRoutine());
    }

    IEnumerator ShootRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(shootInterval);
            ShootShuriken();
        }
    }

    void ShootShuriken()
    {
        GameObject shuriken = Instantiate(shurikenPrefab, player.position, Quaternion.identity);

        // Random góc bắn từ 0 đến 360 độ
        float randomAngle = Random.Range(0f, 360f);
        Vector2 randomDirection = Quaternion.Euler(0, 0, randomAngle) * Vector2.right;

        // Gán hướng bay cho phi tiêu
        BouncingShuriken bouncingShuriken = shuriken.GetComponent<BouncingShuriken>();
        if (bouncingShuriken != null)
        {
            bouncingShuriken.SetDirection(randomDirection);
        }

        Destroy(shuriken, 10f); // Xóa sau 10 giây
    }
}