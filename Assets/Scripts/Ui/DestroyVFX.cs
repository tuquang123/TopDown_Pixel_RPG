using UnityEngine;

public class DestroyVFX : MonoBehaviour
{
    public void OnDestroy()
    {
        Destroy(gameObject);
    }
}
