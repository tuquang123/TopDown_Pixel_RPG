using UnityEngine;

public class DestroyVFX : MonoBehaviour
{
    public void OnDestroy()
    {
        Destroy(gameObject);
    }
    public void OnDeActive()
    {
        gameObject.SetActive(false);
    }
}
