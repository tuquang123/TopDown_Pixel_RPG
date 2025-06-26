using UnityEngine;

public class SkillUIController : BasePopup
{
    public GameObject skillButtonPrefab;
    public Transform skillListContainer;
    public SkillSystem skillSystem;
    public SkillDetailPanel skillDetailPanel;

    public override void Show()
    {
        base.Show();
        RefreshSkillButtons();
        skillDetailPanel.Hide();
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
