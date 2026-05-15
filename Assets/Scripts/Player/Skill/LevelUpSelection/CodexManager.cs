using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPG.Skills.LevelUpSelection
{
    public sealed class CodexManager : MonoBehaviour
    {
        [SerializeField] private List<PassiveSkillData> passiveSkillPool = new();

        private readonly Dictionary<string, SkillInstance> learnedSkills = new(StringComparer.Ordinal);
        private readonly Dictionary<string, PassiveSkillData> poolById = new(StringComparer.Ordinal);

        public event Action<SkillInstance, PassiveSkillData> OnSkillAddedOrUpgraded;

        public IReadOnlyCollection<SkillInstance> LearnedSkills => learnedSkills.Values;

        private void Awake()
        {
            BuildLookup();
        }

        public void BuildLookup()
        {
            poolById.Clear();

            foreach (var data in passiveSkillPool)
            {
                if (data == null || !data.IsValid())
                {
                    continue;
                }

                poolById[data.SkillId] = data;
            }
        }

        public int GetCurrentLevel(string skillId)
        {
            return learnedSkills.TryGetValue(skillId, out var instance) ? instance.level : 0;
        }

        public bool IsMaxed(string skillId)
        {
            if (!learnedSkills.TryGetValue(skillId, out var instance))
            {
                return false;
            }

            return instance.IsMaxed;
        }

        public SkillInstance AddOrUpgrade(PassiveSkillData skillData)
        {
            if (skillData == null || !skillData.IsValid())
            {
                throw new ArgumentException("Skill data is invalid.", nameof(skillData));
            }

            if (learnedSkills.TryGetValue(skillData.SkillId, out var existing))
            {
                existing.Upgrade();
                OnSkillAddedOrUpgraded?.Invoke(existing, skillData);
                return existing;
            }

            var instance = new SkillInstance(skillData.SkillId, 1, skillData.MaxLevel);
            learnedSkills[skillData.SkillId] = instance;
            OnSkillAddedOrUpgraded?.Invoke(instance, skillData);
            return instance;
        }

        public List<PassiveSkillData> GetRandomSelection(int count, System.Random rng = null)
        {
            if (count <= 0)
            {
                return new List<PassiveSkillData>();
            }

            rng ??= new System.Random();

            var notMaxed = passiveSkillPool
                .Where(s => s != null && s.IsValid() && !IsMaxed(s.SkillId))
                .OrderBy(_ => rng.Next())
                .ToList();

            var maxed = passiveSkillPool
                .Where(s => s != null && s.IsValid() && IsMaxed(s.SkillId))
                .OrderBy(_ => rng.Next())
                .ToList();

            var result = new List<PassiveSkillData>(count);

            foreach (var skill in notMaxed)
            {
                if (result.Count >= count)
                {
                    break;
                }

                result.Add(skill);
            }

            foreach (var skill in maxed)
            {
                if (result.Count >= count)
                {
                    break;
                }

                result.Add(skill);
            }

            return result;
        }
    }
}
