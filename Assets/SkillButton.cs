using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    private SkillData _skillData;
    public Button learnButton;
    public Button assignButton;
    private SkillSystem _skillSystem;

    public void Initialize(SkillData data, SkillSystem system)
    {
        _skillData = data;
        _skillSystem = system;
        learnButton.onClick.AddListener(OnLearnButtonClicked);
        assignButton.onClick.AddListener(OnAssignButtonClicked);

        UpdateUI();
    }

    private void UpdateUI()
    {
        learnButton.GetComponentInChildren<Text>().text = _skillData.skillName;
        assignButton.GetComponent<Image>().sprite = _skillData.icon;

        // Kiểm tra xem kỹ năng đã được học chưa, nếu rồi thì vô hiệu hóa nút học
        if (_skillSystem.IsSkillUnlocked(_skillData.skillID))
        {
            learnButton.interactable = false; // Vô hiệu hóa nút học khi kỹ năng đã được học

            // Nếu là kỹ năng chủ động, hiển thị nút gán nếu kỹ năng đã được học
            if (_skillData.skillType == SkillType.Active)
            {
                assignButton.gameObject.SetActive(true); // Hiển thị nút gán
            }
        }
        else
        {
            // Ẩn nút gán nếu kỹ năng chưa được học (cho cả kỹ năng chủ động và thụ động)
            assignButton.gameObject.SetActive(false);
        }

        // Nếu là kỹ năng thụ động, ẩn nút gán vì kỹ năng thụ động không cần gán vào slot
        if (_skillData.skillType == SkillType.Passive)
        {
            assignButton.gameObject.SetActive(false); // Ẩn nút gán đối với kỹ năng thụ động
        }
    }

    private void OnLearnButtonClicked()
    {
        if (_skillSystem != null && _skillData != null)
        {
            if (_skillSystem.CanUnlockSkill(_skillData.skillID)) // Kiểm tra nếu có đủ điểm kỹ năng
            {
                _skillSystem.UnlockSkill(_skillData.skillID);
                _skillSystem.DecrementSkillPoint(); // Giảm điểm kỹ năng sau khi học

                // Nếu kỹ năng là thụ động, áp dụng ngay vào stats
                if (_skillData.skillType == SkillType.Passive)
                {
                    _skillSystem.UseSkill(_skillData.skillID); // Áp dụng kỹ năng thụ động vào stats
                    Debug.Log($"Đã học và áp dụng kỹ năng thụ động: {_skillData.skillName} vào stats");
                }
                else
                {
                    Debug.Log($"Đã học kỹ năng chủ động: {_skillData.skillName}");
                }

                UpdateUI(); // Cập nhật UI sau khi học kỹ năng
            }
            else
            {
                Debug.Log("Không đủ điểm kỹ năng để học kỹ năng này.");
            }
        }
    }

    private void OnAssignButtonClicked()
    {
        if (_skillSystem != null)
        {
            int availableSlot = _skillSystem.GetFirstAvailableSlot();
            if (availableSlot != -1)
            {
                _skillSystem.AssignSkillToSlot(availableSlot, _skillData.skillID);
                Debug.Log($"Gán kỹ năng {_skillData.skillName} vào ô {availableSlot + 1}");
            }
        }
    }
}

