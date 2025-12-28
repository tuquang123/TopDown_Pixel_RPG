using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkillUIController : BasePopup
{
    public GameObject skillButtonPrefab;
    public Transform skillListContainer;
    public SkillSystem skillSystem;
    public SkillDetailPanel skillDetailPanel;
    [SerializeField] private ScrollRect scrollRect;

    // 🔥 THÊM
    private void OnEnable()
    {
        SkillDetailPanel.OnSkillChanged += RefreshSkillButtons;
    }

    private void OnDisable()
    {
        SkillDetailPanel.OnSkillChanged -= RefreshSkillButtons;
    }

    public override void Show()
    {
        base.Show();
        RefreshSkillButtons();
        skillDetailPanel.Hide();
        StartCoroutine(ResetScrollPositionDelayed());
    }

    private IEnumerator ResetScrollPositionDelayed()
    {
        yield return null;
        yield return null;
        scrollRect.verticalNormalizedPosition = 1f;
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