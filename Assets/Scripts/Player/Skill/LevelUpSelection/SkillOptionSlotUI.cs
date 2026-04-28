using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.Skills.LevelUpSelection
{
    public sealed class SkillOptionSlotUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text skillName;
        [SerializeField] private TMP_Text description;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private Button pickButton;

        private PassiveSkillData skillData;
        private System.Action<PassiveSkillData> onPick;

        public void Bind(PassiveSkillData data, int currentLevel, System.Action<PassiveSkillData> onPickCallback)
        {
            skillData = data;
            onPick = onPickCallback;

            icon.sprite = data.Icon;
            skillName.text = data.DisplayName;
            description.text = data.ShortDescription;
            levelText.text = $"Lv {currentLevel}/{data.MaxLevel}";

            pickButton.onClick.RemoveAllListeners();
            pickButton.onClick.AddListener(HandlePick);
        }

        private void HandlePick()
        {
            onPick?.Invoke(skillData);
        }
    }
}
