using UnityEngine;

public class DevPanelSpawner : MonoBehaviour
{
    [SerializeField] private GameObject devPanelPrefab;
    [SerializeField] private Transform canvasParent;

    private GameObject currentPanel;

    public void OpenDevPanel()
    {
        if (currentPanel != null)
            return; // tránh spawn nhiều cái

        currentPanel = Instantiate(devPanelPrefab, canvasParent);
    }

    public void CloseDevPanel()
    {
        if (currentPanel != null)
        {
            Destroy(currentPanel);
            currentPanel = null;
        }
    }
}