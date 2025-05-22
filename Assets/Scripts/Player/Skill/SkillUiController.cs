using UnityEngine;

public class SkillUIController : MonoBehaviour
{
    public GameObject skillButtonPrefab;
    public Transform skillListContainer;
    public SkillSystem skillSystem;

    private void OnEnable()
    {
        // Chỉ xóa những item có SkillButton (tránh xóa BG hoặc layout cố định)
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
            skillButton.Initialize(skillData, skillSystem);
        }
    }

}