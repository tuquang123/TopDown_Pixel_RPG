using UnityEngine;

public class FloatingTextSpawner : Singleton<FloatingTextSpawner>
{
    [Header("Floating Text Settings")]
    public GameObject floatingTextPrefab;
    public Transform floatingTextCanvas; 
    public Camera mainCam;

    protected override void Awake()
    {
        base.Awake();

        if (mainCam == null)
        {
            mainCam = Camera.main;
        }
    }

    public void SpawnText(string text, Vector3 worldPosition, Color color)
    {
        if (floatingTextPrefab == null || floatingTextCanvas == null || mainCam == null)
        {
            Debug.LogWarning("FloatingTextSpawner thiếu thiết lập (prefab/canvas/camera).");
            return;
        }

        Vector3 screenPosition = mainCam.WorldToScreenPoint(worldPosition);

        GameObject go = Instantiate(floatingTextPrefab, floatingTextCanvas);
        go.transform.position = screenPosition;

        if (go.TryGetComponent(out FloatingText floatingText))
        {
            floatingText.Setup(text, color);
        }
        else
        {
            Debug.LogWarning("Prefab không có component FloatingText.");
        }
    }
}