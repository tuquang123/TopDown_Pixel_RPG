using System;
using UnityEngine;

public class StageManager : Singleton<StageManager>
{
    [Header("Stage")]
    [SerializeField, Min(1)] private int currentStage = 1;

    [Header("Scaling")]
    [SerializeField, Min(0f)] private float healthScalePerStage = 0.2f;
    [SerializeField, Min(0f)] private float damageScalePerStage = 0.15f;
    [SerializeField, Min(0f)] private float moveSpeedScalePerStage = 0.03f;
    [SerializeField, Min(0f)] private float attackCooldownReductionPerStage = 0.01f;
    [SerializeField, Min(0.05f)] private float minAttackCooldown = 0.2f;

    public event Action<int> OnStageChanged;

    public int CurrentStage => currentStage;

    public void AdvanceStage()
    {
        currentStage++;
        OnStageChanged?.Invoke(currentStage);
    }

    public EnemyLevelData GetScaledData(EnemyLevelData baseData)
    {
        if (baseData == null)
            return null;

        int stageIndex = Mathf.Max(0, currentStage - 1);

        return new EnemyLevelData
        {
            level = baseData.level,
            maxHealth = Mathf.Max(1, Mathf.RoundToInt(baseData.maxHealth * (1f + stageIndex * healthScalePerStage))),
            attackDamage = Mathf.Max(1, Mathf.RoundToInt(baseData.attackDamage * (1f + stageIndex * damageScalePerStage))),
            moveSpeed = Mathf.Max(0.1f, baseData.moveSpeed * (1f + stageIndex * moveSpeedScalePerStage)),
            attackRange = baseData.attackRange,
            detectionRange = baseData.detectionRange,
            attackCooldown = Mathf.Max(minAttackCooldown, baseData.attackCooldown * (1f - stageIndex * attackCooldownReductionPerStage))
        };
    }
}
