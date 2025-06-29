using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISkill
{
    void ExecuteSkill(PlayerStats playerStats, SkillData data);
    bool CanUse(PlayerStats playerStats, SkillData data);
}
public class SkillFactory
{
    public static ISkill CreateSkill(SkillID skillID)
    {
        switch (skillID)
        {
            case SkillID.ShurikenThrow:
                return new ShurikenThrowSkill();
            case SkillID.DamageBoost:
                return new DamageBoost();
            case SkillID.Dash:
                return new DashSkill();
            case SkillID.Slash:
                return new SlashSkill();
            case SkillID.Invincible:
                return new InvincibleSkill();
            
                    // Boost stat skills
            case SkillID.HealthBoost:
                return new HealthBoost();
            case SkillID.ManaBoost:
                return new ManaBoost();
            case SkillID.AttackBoost:
                return new DamageBoost();
            case SkillID.DefenseBoost:
                return new DefenseBoost();
            case SkillID.SpeedBoost:
                return new SpeedBoost();
            case SkillID.CritChanceBoost:
                return new CriticalBoost();
            case SkillID.AttackSpeedBoost:
                return new AttackSpeedBoost();
            
            default:
                throw new ArgumentException("Không tìm thấy kỹ năng với ID này.");
        }
    }
}

public class SkillSystem : MonoBehaviour
{
    public List<SkillData> skillList = new();
    private PlayerStats _playerStats;
    private Dictionary<SkillID, float> skillCooldownTimes = new();
    private Dictionary<int, SkillID> assignedSkills = new();
    public event Action<int, SkillID> OnSkillAssigned;
    public bool autoCastEnabled = true;
    public PlayerStats PlayerStats => _playerStats ; 
    
    private Dictionary<SkillID, SkillState> unlockedSkills = new();

    private void Start()
    {
        _playerStats = GetComponent<PlayerStats>();
        StartCoroutine(AutoCastRoutine());
    }

    private bool isCasting = false;

    private IEnumerator AutoCastRoutine()
    {
        if (_playerStats.isDead) yield break;
        
        while (true)
        {
            if (autoCastEnabled && !isCasting)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (assignedSkills.ContainsKey(i))
                    {
                        SkillID skillID = assignedSkills[i];
                        if (skillID != SkillID.None && CanUseSkill(skillID))
                        {
                            StartCoroutine(ExecuteSkillWithLock(skillID));
                            break; 
                        }
                    }
                }
            }

            yield return new WaitForSeconds(0.1f); 
        }
    }

    private IEnumerator ExecuteSkillWithLock(SkillID skillID)
    {
        isCasting = true;
        UseSkill(skillID);
        
        yield return new WaitForSeconds(1.5f); 

        isCasting = false;
    }
    public bool UnlockSkill(SkillID skillID)
    {
        SkillData data = GetSkillData(skillID);
        if (data == null) return false;

        if (!unlockedSkills.ContainsKey(skillID))
        {
            unlockedSkills[skillID] = new SkillState(skillID, data);
            _playerStats.SetSkillLevel(skillID, 1); // Cập nhật cấp độ trong PlayerStats
            return true;
        }

        SkillState state = unlockedSkills[skillID];
        if (state.level < data.maxLevel)
        {
            state.level++;
            _playerStats.SetSkillLevel(skillID, state.level); 
            return true;
        }

        return false;
    }
    
    public SkillData GetSkillData(SkillID skillID)
    {
        return skillList.Find(s => s.skillID == skillID);
    }

    public void AssignSkillToSlot(int slotIndex, SkillID skillID)
    {
        if (slotIndex >= 0 && slotIndex < 5 && unlockedSkills.ContainsKey(skillID))
        {
            if (assignedSkills.ContainsValue(skillID))
            {
                Debug.Log($"Kỹ năng {skillID} đã được gán vào một ô khác.");
                return;
            }

            if (assignedSkills.ContainsKey(slotIndex) && assignedSkills[slotIndex] != SkillID.None)
            {
                Debug.Log($"Ô {slotIndex + 1} đã có kỹ năng.");
                return;
            }

            assignedSkills[slotIndex] = skillID;
            OnSkillAssigned?.Invoke(slotIndex, skillID);
            Debug.Log($"Đã gán kỹ năng {skillID} vào ô {slotIndex + 1}");
        }
        else
        {
            Debug.Log("Kỹ năng chưa được mở khóa hoặc slot không hợp lệ.");
        }
    }

    public void DecrementSkillPoint()
    {
        if (_playerStats.skillPoints > 0)
        {
            _playerStats.skillPoints--;
            Debug.Log($"Điểm kỹ năng còn lại: {_playerStats.skillPoints}");
        }
        else
        {
            Debug.Log("Không còn điểm kỹ năng.");
        }
    }
    
    public bool CanUnlockSkill(SkillID skillID)
    {
        SkillData data = skillList.Find(s => s.skillID == skillID);
        SkillState state = GetSkillState(skillID);

        int currentLevel = state?.level ?? 0;
        return _playerStats.skillPoints > 0 && currentLevel < data.maxLevel;
    }

    public SkillID GetAssignedSkill(int slotIndex)
    {
        return assignedSkills.ContainsKey(slotIndex) ? assignedSkills[slotIndex] : SkillID.None;
    }
    
    private SkillLevelStat GetCurrentLevelStat(SkillID skillID)
    {
        SkillState skillState = GetSkillState(skillID);
        if (skillState == null) return null;

        SkillData skillData = GetSkillData(skillID);
        if (skillData == null) return null;

        return skillData.GetLevelStat(skillState.level);
    }
    

    public bool CanUseSkill(SkillID skillID)
    {
        if (!IsSkillOffCooldown(skillID)) return false;
    
        SkillLevelStat currentLevelStat = GetCurrentLevelStat(skillID);
        if (currentLevelStat == null) return false;
    
        if (_playerStats.currentMana < currentLevelStat.manaCost)
            return false;
    
        SkillData skillData = GetSkillData(skillID);
        ISkill skillInstance = SkillFactory.CreateSkill(skillID);
        
        return skillInstance.CanUse(_playerStats, skillData);
    }
    
    public bool IsSkillOffCooldown(SkillID skillID)
    {
        SkillLevelStat levelStat = GetCurrentLevelStat(skillID);
        if (levelStat == null) return false;
    
        return Time.time >= skillCooldownTimes.GetValueOrDefault(skillID, -Mathf.Infinity) + levelStat.cooldown;
    }

    
    public event Action<SkillID> OnSkillUsed;

    public void UseSkill(SkillID skillID)
    {
        SkillLevelStat currentLevelStat = GetCurrentLevelStat(skillID);
        if (currentLevelStat == null) return;
        
        if (Time.time < skillCooldownTimes.GetValueOrDefault(skillID, -Mathf.Infinity) + currentLevelStat.cooldown)
            return;
        
        if (_playerStats.currentMana < currentLevelStat.manaCost)
        {
            Debug.Log("Không đủ mana.");
            return;
        }

        ISkill skillInstance = SkillFactory.CreateSkill(skillID);
        SkillData skillData = GetSkillData(skillID);
        if (skillInstance.CanUse(_playerStats, skillData))
        {
            _playerStats.UseMana(currentLevelStat.manaCost);
            skillCooldownTimes[skillID] = Time.time;
            skillInstance.ExecuteSkill(_playerStats, skillData);

            OnSkillUsed?.Invoke(skillID);
        }
    }
    
    public int GetSkillLevel(SkillID skillID)
    {
        if (unlockedSkills.TryGetValue(skillID, out SkillState state))
            return state.level;
        return 0;
    }
    
    public int GetFirstAvailableSlot()
    {
        for (int i = 0; i < 5; i++)
        {
            if (!assignedSkills.ContainsKey(i) || assignedSkills[i] == SkillID.None)
            {
                return i; 
            }
        }
        return -1; 
    }

    public bool IsSkillUnlocked(SkillID skillID)
    {
        return unlockedSkills.ContainsKey(skillID);
    }

    public SkillState GetSkillState(SkillID skillID)
    {
        if (unlockedSkills.TryGetValue(skillID, out SkillState state))
        {
            return state;
        }
        return null; 
    }
}