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

    public override void Show()
    {
        base.Show();
        RefreshSkillButtons();
        skillDetailPanel.Hide();
        StartCoroutine(ResetScrollPositionDelayed());
    }

    private IEnumerator ResetScrollPositionDelayed()
    {
        yield return null; // Frame 1: chờ layout update
        yield return null; // Frame 2: đảm bảo layout + content size đã hoàn chỉnh

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
            GameObject skillButtonObject = Instantiate(skillButtonPrefab, skillListContainer);
            SkillButton skillButton = skillButtonObject.GetComponent<SkillButton>();
            skillButton.Initialize(skillData, skillSystem, skillDetailPanel);
        }
    }
}
