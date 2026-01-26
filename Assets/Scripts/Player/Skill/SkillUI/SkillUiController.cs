using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUIController : BasePopup
{
    public GameObject skillButtonPrefab;
    public Transform skillListContainer;
    SkillSystem skillSystem;
    public SkillDetailPanel skillDetailPanel;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private TextMeshProUGUI skillPointText;

    // 🔥 THÊM
    private void OnEnable()
    {
        if (skillSystem == null)
            skillSystem = CommonReferent.Instance.playerPrefab.GetComponent<SkillSystem>();

        SkillDetailPanel.OnSkillChanged += OnSkillChanged;
    }


    private void OnDisable()
    {
        SkillDetailPanel.OnSkillChanged -= OnSkillChanged;
    }

    private void OnSkillChanged()
    {
        RefreshSkillButtons();
        RefreshSkillPoint();   
    }

    private void RefreshSkillPoint()
    {
        if (skillSystem == null || skillPointText == null)
            return;

        skillPointText.text = $"Skill Point : {PlayerStats.Instance.skillPoints}";

    }


    public override void Show()
    {
        base.Show();
        skillSystem = CommonReferent.Instance.playerPrefab.GetComponent<SkillSystem>();
        RefreshSkillButtons();
        skillDetailPanel.Hide();
        StartCoroutine(ResetScrollPositionDelayed());
        RefreshSkillPoint();
    }

    private IEnumerator ResetScrollPositionDelayed()
    {
        yield return null;
        yield return null;
        scrollRect.verticalNormalizedPosition = 1f;
    }
    public void Close()
    {
        UIManager.Instance.HidePopupByType(PopupType.Skill);
    }
    private void RefreshSkillButtons()
    {
        foreach (Transform child in skillListContainer)
        {
            if (child.GetComponent<SkillButton>() != null)
            {
                Destroy(child.gameObject);
            }
        }

        foreach (SkillData skillData in skillSystem.skillList)
        {
            GameObject obj = Instantiate(skillButtonPrefab, skillListContainer);
            SkillButton btn = obj.GetComponent<SkillButton>();
            btn.Initialize(skillData, skillSystem, skillDetailPanel);
        }
    }
}