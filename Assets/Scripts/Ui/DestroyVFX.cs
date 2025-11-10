using UnityEngine;

public class DestroyVFX : MonoBehaviour, IPooledObject
{
    public float lifetime = 2f;
    private float timer;

    public void OnObjectSpawn() => timer = lifetime;

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
            gameObject.SetActive(false);
    }

    // Call to Animation
    public void OnDeActive() => gameObject.SetActive(false);
}