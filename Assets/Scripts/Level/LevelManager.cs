﻿using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    public enum TravelDirection
    {
        Forward,
        Backward,
        Default
    }

    [SerializeField] private LevelDatabase levelDatabase;
    [SerializeField] private GameObject player;

    private int currentLevel = 0;
    private GameObject currentLevelInstance;
    
    void Start()
    {
        LoadLevel(currentLevel);
    }
    
    public void LoadSpecificLevel(int index, TravelDirection direction)
    {
        if (index < 0 || index >= levelDatabase.TotalLevels)
        {
            Debug.LogWarning("Level index không hợp lệ.");
            return;
        }

        currentLevel = index;
        LoadLevel(currentLevel, direction);
    }
    
    public void NextLevel()
    {
        currentLevel++;
        if (currentLevel >= levelDatabase.TotalLevels)
            currentLevel = 0;

        LoadLevel(currentLevel, TravelDirection.Forward);
    }

    public void PreviousLevel()
    {
        currentLevel--;
        if (currentLevel < 0)
            currentLevel = levelDatabase.TotalLevels - 1;

        LoadLevel(currentLevel, TravelDirection.Backward);
    }
    
    [SerializeField] private ScreenFader screenFader;

    private void LoadLevel(int index , TravelDirection direction = TravelDirection.Default)
    {
        screenFader.FadeIn(0.5f, () =>
        {
            if (currentLevelInstance != null)
            {
                ClearAllEnemyPools();
                Destroy(currentLevelInstance);
            }

            EnemyTracker.Instance.ClearAllEnemies();
            
            var levelData = levelDatabase.GetLevel(index);
            if (levelData == null) return;

            currentLevelInstance = Instantiate(levelData.levelPrefab);

            Vector3 targetPos = direction switch
            {
                TravelDirection.Forward => levelData.entryFromPreviousLevel,
                TravelDirection.Backward => levelData.entryFromNextLevel,
                _ => Vector3.zero
            };
        
            player.transform.position = targetPos;
            PositionAlliesAroundPlayer(targetPos);

            foreach (var sp in currentLevelInstance.GetComponentsInChildren<SpawnPoint>())
            {
                var levelDB = CommonReferent.Instance.levelDatabase;
                
                sp.Spawn(levelDB);
            }

            Debug.Log($"Đã load level {index}: {levelData.levelName}");

            screenFader.FadeOut(0.5f);
        });
    }
    public void ResetLevel()
    {
        LoadLevel(currentLevel);
    }
    
    private void ClearAllEnemyPools()
    {
        if (currentLevelInstance == null) return;
        
        var enemyPrefabs = currentLevelInstance.GetComponentsInChildren<SpawnPoint>();
        foreach (var sp in enemyPrefabs)
        {
            if (sp.enemyPrefab != null)
            {
                ObjectPooler.Instance.ClearPool(sp.enemyPrefab.name);
            }
        }
    }
    private void PositionAlliesAroundPlayer(Vector3 center)
    {
        foreach (var ally in AllyManager.Instance.GetAllies())
        {
            if (ally == null) continue;

            Vector2 offset = AllyManager.Instance.GetOffsetPosition(ally);
            ally.transform.position = center + (Vector3)offset;
        }
    }


}