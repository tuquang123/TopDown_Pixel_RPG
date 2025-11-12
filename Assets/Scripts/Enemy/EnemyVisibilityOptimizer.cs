using UnityEngine;

public class EnemyVisibilityOptimizer : MonoBehaviour
{
    private Renderer[] renderers;

    private void Awake()
    {
        // Lấy tất cả renderer trên object và con
        renderers = GetComponentsInChildren<Renderer>();
    }
    private void Reset()
    {
        // Lấy tất cả renderer trên object và con
        renderers = GetComponentsInChildren<Renderer>();
    }

    public void SetVisible(bool isVisible)
    {
        foreach (var r in renderers)
            r.enabled = isVisible;
    }

    private void Update()
    {
        // VD: tắt render nếu enemy ra ngoài camera
        if (Camera.main == null) return;

        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        bool onScreen = viewportPos.x >= 0 && viewportPos.x <= 1 &&
                        viewportPos.y >= 0 && viewportPos.y <= 1 &&
                        viewportPos.z > 0;

        SetVisible(onScreen);
    }
}