using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI label;
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

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        detailPanel.Setup(skillData, skillSystem);
    }
}