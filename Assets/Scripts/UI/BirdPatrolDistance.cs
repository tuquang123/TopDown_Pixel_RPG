using UnityEngine;

public class BirdPatrolDistance : MonoBehaviour
{
    [Header("Fly Settings")]
    public float speed = 2f;
    public float maxDistance = 5f;   // quãng đường bay tính từ điểm gốc

    private Vector3 startPos;
    private bool movingRight = true;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // Bay qua phải
        if (movingRight)
        {
            transform.position += Vector3.right * speed * Time.deltaTime;

            if (transform.position.x >= startPos.x + maxDistance)
            {
                movingRight = false;
                Flip(false); // quay mặt sang trái
            }
        }
        // Bay qua trái
        else
        {
            transform.position += Vector3.left * speed * Time.deltaTime;

            if (transform.position.x <= startPos.x - maxDistance)
            {
                movingRight = true;
                Flip(true); // quay mặt sang phải
            }
        }
    }

    private void Flip(bool faceRight)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (faceRight ? 1 : -1);
        transform.localScale = scale;
    }
}