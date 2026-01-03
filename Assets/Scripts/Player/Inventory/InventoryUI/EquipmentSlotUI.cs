using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour
{
    public ItemIconHandler icon;
    public Image iconDefault;
    public Image background; // <-- đổi theo tier
    public Button button;
    public Button iconButton;
    public Image selectedIcon; // overlay / viền sáng

    private bool isSelected;
    private ItemInstance currentItem;

    private void Awake()
    {
        if (selectedIcon != null)
            selectedIcon.gameObject.SetActive(false);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (selectedIcon != null)
            selectedIcon.gameObject.SetActive(selected);

        // scale nhẹ khi chọn
        transform.localScale = selected ? Vector3.one * 1.04f : Vector3.one;
    }
}