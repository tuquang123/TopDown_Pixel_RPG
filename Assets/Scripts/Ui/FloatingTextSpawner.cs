using UnityEngine;

public class FloatingTextSpawner : MonoBehaviour
{
    public static FloatingTextSpawner Instance;

    public GameObject floatingTextPrefab;
    public Transform floatingTextCanvas; // gán canvas ở scene
    public Camera mainCam; // gán maincamera

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnText(string text, Vector3 worldPosition, Color color)
    {
        Vector3 screenPosition = mainCam.WorldToScreenPoint(worldPosition);

        var go = Instantiate(floatingTextPrefab, floatingTextCanvas);
        go.transform.position = screenPosition;

        var floatingText = go.GetComponent<FloatingText>();
        floatingText.Setup(text, color);
    }
}