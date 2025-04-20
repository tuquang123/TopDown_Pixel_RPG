using UnityEngine;

public class Shuriken : MonoBehaviour
{
    public Transform target;  // Nhân vật trung tâm
    public float radius = 2f; // Khoảng cách từ nhân vật
    public float orbitSpeed = 100f; // Tốc độ quay quanh nhân vật
    public float selfRotateSpeed = 200f; // Tốc độ xoay quanh chính nó
    public int index = 0; // Chỉ số của phi tiêu
    public int totalShurikens = 1; // Tổng số phi tiêu
    public int damage = 10;

    private float angleOffset;

    void Start()
    {
        UpdatePosition();
    }

    void Update()
    {
        if (target == null) return;

        // Xoay quanh nhân vật
        float angle = Time.time * orbitSpeed + angleOffset;
        float x = target.position.x + Mathf.Cos(angle) * radius;
        float y = target.position.y + Mathf.Sin(angle) * radius;
        
        transform.position = new Vector3(x, y, target.position.z);

        // Xoay quanh chính nó
        transform.Rotate(Vector3.forward, selfRotateSpeed * Time.deltaTime);
    }

    public void SetIndex(int index, int total)
    {
        this.index = index;
        this.totalShurikens = total;
        angleOffset = (index / (float)total) * Mathf.PI * 2;
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (target == null) return;
        
        float angle = angleOffset;
        float x = target.position.x + Mathf.Cos(angle) * radius;
        float y = target.position.y + Mathf.Sin(angle) * radius;

        transform.position = new Vector3(x, y, target.position.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyAI>()?.TakeDamage(damage);
        }
    }
}