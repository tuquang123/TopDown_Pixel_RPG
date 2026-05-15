using System;

namespace RPG.Skills.LevelUpSelection
{
    [Serializable]
    public sealed class SkillInstance
    {
        public string skillId;
        public int level;
        public int maxLevel;

        public SkillInstance(string skillId, int level, int maxLevel)
        {
            this.skillId = skillId;
            this.level = level;
            this.maxLevel = maxLevel;
        }

        public bool IsMaxed => level >= maxLevel;

        public void Upgrade()
        {
            level = Math.Min(level + 1, maxLevel);
        }
    }
}
