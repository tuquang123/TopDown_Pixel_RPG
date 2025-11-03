using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private TextMeshProUGUI levelText; // 👈 thêm dòng này
    [SerializeField] private Image iconImage;

    private SkillData skillData;
    private SkillSystem skillSystem;
    private SkillDetailPanel detailPanel;

    public void Initialize(SkillData data, SkillSystem system, SkillDetailPanel detail)
    {
        skillData = data;
        skillSystem = system;
        detailPanel = detail;

        label.text = data.skillName;
        iconImage.sprite = data.icon;

        // 👇 Hiển thị cấp độ (ví dụ: 1/10)
        int currentLevel = system.GetSkillLevel(data.skillID); // hoặc data.currentLevel nếu có sẵn
        int maxLevel = data.maxLevel; // hoặc giá trị cố định nếu chưa có trong data
        levelText.text = $"{currentLevel}/{maxLevel}";

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        detailPanel.Setup(skillData, skillSystem);
    }
}