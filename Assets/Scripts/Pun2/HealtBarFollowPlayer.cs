using UnityEngine;

public class HealthBarFollowPlayer : MonoBehaviour
{
    [Header("Settings")]
    public Transform target; // Player transform
    public Vector3 offset = new Vector3(0, 1.5f, 0); // Offset phía trên đầu player
    
    [Header("Rotation Lock")]
    public bool lockRotation = true; // Luôn giữ Canvas thẳng
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // Follow player position
        transform.position = target.position + offset;
        
        // Luôn giữ Canvas thẳng (không lật theo player)
        if (lockRotation)
        {
            transform.rotation = Quaternion.identity;
        }
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}