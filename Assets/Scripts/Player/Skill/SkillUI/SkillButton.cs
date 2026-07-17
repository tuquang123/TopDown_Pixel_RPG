using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image iconImage;

    [Header("Lock UI")]
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private Image dimImage;

    private SkillData skillData;
    private SkillSystem skillSystem;
    private SkillDetailPanel detailPanel;

    private void OnEnable()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged += RefreshLocalizedText;
        }

        RefreshLocalizedText();
    }

    private void OnDisable()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged -= RefreshLocalizedText;
        }
    }

    public void Initialize(SkillData data, SkillSystem system, SkillDetailPanel detail)
    {
        skillData = data;
        skillSystem = system;
        detailPanel = detail;

        RefreshLocalizedText();
        iconImage.sprite = data.icon;

        Refresh();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        // 🔥 CHỈ GỌI SETUP – KHÔNG ĐỤNG LOGIC LOCK
        detailPanel.Setup(skillData, skillSystem);
    }

    public void Refresh()
    {
        RefreshLevelText();
        RefreshLockState();
    }

    private void RefreshLevelText()
    {
        int currentLevel = skillSystem.GetSkillLevel(skillData.skillID);
        levelText.text = $"{currentLevel}/{skillData.maxLevel}";
    }

    private void RefreshLocalizedText()
    {
        if (skillData != null && label != null)
        {
            label.text = skillData.skillName;
        }
    }

    private void RefreshLockState()
    {
        bool isUnlocked = skillSystem.IsSkillUnlocked(skillData.skillID);

        lockOverlay.SetActive(!isUnlocked);
        dimImage.enabled = !isUnlocked;

        // vẫn cho click để xem info
        button.interactable = true;
    }
    
    
    
    
}
