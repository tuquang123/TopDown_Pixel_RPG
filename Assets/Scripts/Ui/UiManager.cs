using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject inventoryPanel;  // Panel chứa Inventory và Equipment
    public Button toggleButton;        // Nút mở/tắt UI

    private bool isUIOpen = false;     // Trạng thái của UI

    private void Start()
    {
        // Ẩn panel khi bắt đầu
        inventoryPanel.SetActive(false);

        // Gán sự kiện cho nút bấm
        toggleButton.onClick.AddListener(ToggleUI);
    }

    private void ToggleUI()
    {
        isUIOpen = !isUIOpen;
        inventoryPanel.SetActive(isUIOpen);
    }
}