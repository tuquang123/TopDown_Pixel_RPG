using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillDisplayUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI typeTagText;
    public Image backgroundImage;        // Background để highlight

    private System.Action<int> onClickCallback;
    private int myIndex = -1;

    public void DisplaySkill(SkillData skill, int index, System.Action<int> callback)
    {
        if (skill == null) return;

        myIndex = index;
        onClickCallback = callback;

        if (iconImage != null && skill.icon != null)
            iconImage.sprite = skill.icon;

        if (nameText != null) nameText.text = skill.skillName;
        if (descText != null) descText.text = skill.descriptionTemplate;
        if (typeTagText != null) typeTagText.text = "Passive";

        // Thêm sự kiện click vào toàn bộ panel
        Button btn = GetComponent<Button>() ?? gameObject.AddComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onClickCallback?.Invoke(myIndex));
    }

    public void SetSelected(bool isSelected)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected 
                ? new Color(0.2f, 1f, 0.4f, 0.25f)   // Màu highlight xanh nhạt
                : new Color(1f, 1f, 1f, 0.1f);       // Màu bình thường
        }
    }
}