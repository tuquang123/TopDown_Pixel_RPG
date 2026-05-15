using UnityEngine;

namespace RPG.Skills.LevelUpSelection
{
    [CreateAssetMenu(fileName = "PassiveSkill_", menuName = "RPG/Skills/Passive Skill Data")]
    public sealed class PassiveSkillData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string skillId;
        [SerializeField] private string displayName;
        [SerializeField, TextArea(2, 4)] private string shortDescription;

        [Header("Presentation")]
        [SerializeField] private Sprite icon;

        [Header("Progression")]
        [SerializeField, Min(1)] private int maxLevel = 5;

        public string SkillId => skillId;
        public string DisplayName => displayName;
        public string ShortDescription => shortDescription;
        public Sprite Icon => icon;
        public int MaxLevel => maxLevel;

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(skillId) && maxLevel > 0;
        }
    }
}
