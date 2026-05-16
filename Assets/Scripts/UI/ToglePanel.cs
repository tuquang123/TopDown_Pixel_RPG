using UnityEngine;
using UnityEngine.UI;

public class ToglePanel : MonoBehaviour
{
    public GameObject inventoryPanel;  
    public Button toggleButton;       

    private bool isUIOpen = false;     

    private void Start()
    {
        inventoryPanel.SetActive(false);
        toggleButton.onClick.AddListener(ToggleUI);
    }

    private void ToggleUI()
    {
        isUIOpen = !isUIOpen;
        inventoryPanel.SetActive(isUIOpen);
    }
}