using UnityEngine;

public class SkillUIController : MonoBehaviour
{
    public GameObject skillButtonPrefab;
    public Transform skillListContainer;
    public SkillSystem skillSystem;

    private void OnEnable()
    {
        // Clear UI trước khi cập nhật lại
        foreach (Transform child in skillListContainer)
        {
            Destroy(child.gameObject);
        }

        // Hiển thị các kỹ năng
        foreach (SkillData skillData in skillSystem.skillList)
        {
            GameObject skillButtonObject = Instantiate(skillButtonPrefab, skillListContainer);
            SkillButton skillButton = skillButtonObject.GetComponent<SkillButton>();

            skillButton.Initialize(skillData, skillSystem);
        }
    }
}