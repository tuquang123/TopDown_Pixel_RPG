using UnityEngine;

public class SkillUIController : MonoBehaviour
{
    public GameObject skillButtonPrefab;
    public Transform skillListContainer;
    public SkillSystem skillSystem;
    public SkillDetailPanel skillDetailPanel;

    private void OnEnable()
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

        skillDetailPanel.Hide(); 
    }
}