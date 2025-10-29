using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour
{
    public ItemIconHandler icon;
    public Image iconDefault;
    public Image background;
    public Button button;
    public Button iconButton;
    public Image selectedIcon; // overlay / viền sáng

    private bool isSelected;

    private void Awake()
    {
        if (selectedIcon != null)
            selectedIcon.gameObject.SetActive(false);
    }

    // <-- Thêm method này để có thể gọi như currentSelectedSlot.SetSelected(true)
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (selectedIcon != null)
            selectedIcon.gameObject.SetActive(selected);

        // ví dụ: scale nhẹ khi chọn
        transform.localScale = selected ? Vector3.one * 1.04f : Vector3.one;

        // tuỳ chỉnh màu nền nếu muốn (lưu ý: giữ nguyên màu theo tier khi có item)
        if (!selected)
        {
            // nothing here if you prefer to keep background set by UpdateEquipmentUI
        }
        else
        {
            // nếu muốn override background khi chọn, uncomment:
            // background.color = new Color32(255, 230, 180, 255);
        }
    }
}