using UnityEngine;

public class DevPanelToggleButton : MonoBehaviour
{
    [SerializeField] private GameObject devPanel;

    public void ToggleDevPanel()
    {
        if (devPanel == null) return;

        devPanel.SetActive(!devPanel.activeSelf);
    }
}