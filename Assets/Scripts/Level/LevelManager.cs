using System;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    public enum TravelDirection
    {
        Forward,
        Backward,
        Default
    }

    LevelDatabase levelDatabase;
    GameObject player;

    private int currentLevel = 0;
    private GameObject currentLevelInstance;
    private bool isLoadingFromSave = false;
    private Vector3 savedPlayerPosition;

    void Start()
    {
        levelDatabase = CommonReferent.Instance.levelDatabase;
        player = CommonReferent.Instance.playerPrefab;
        LoadGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
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
        if (screenFader == null)
        {
            Debug.LogError("ScreenFader NULL → load level trực tiếp");
            // Không cần InternalLoadLevel
        }
        {
            if (currentLevelInstance != null)
            {
                EnemyTracker.Instance.ClearAllEnemies();
                
                ObjectPooler.Instance.ClearAllPools();

                Destroy(currentLevelInstance); 
            }
            
            var levelData = levelDatabase.GetLevel(index);
            if (levelData == null) return;

            currentLevelInstance = Instantiate(levelData.levelPrefab);

            Vector3 targetPos = direction switch
            {
                TravelDirection.Forward => levelData.entryFromPreviousLevel,
                TravelDirection.Backward => levelData.entryFromNextLevel,
                _ => CommonReferent.Instance.defaultEntryPosition
            };
        
            Vector3 finalPos = isLoadingFromSave ? savedPlayerPosition : targetPos;

            player.transform.position = finalPos;
            PositionAlliesAroundPlayer(finalPos);

            isLoadingFromSave = false;


            foreach (var sp in currentLevelInstance.GetComponentsInChildren<SpawnPoint>())
            {
                var levelDB = CommonReferent.Instance.enemyLevelDatabase;
                
                sp.Spawn(levelDB);
            }
            
            Debug.Log($"Đã load level {index}: {levelData.levelName}");

            screenFader.FadeOut(0.5f);
        };
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
                ObjectPooler.Instance.ClearAllPools();
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
    [System.Serializable]
    private class SaveData
    {
        public int levelIndex;
        public Vector3 playerPosition;
    }
    public void SaveGame()
    {
        SaveData data = new SaveData
        {
            levelIndex = currentLevel,
            playerPosition = player.transform.position
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SAVE_DATA", json);
        PlayerPrefs.Save();

        Debug.Log("Save game thành công");
    }
    
    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey("SAVE_DATA"))
        {
            Debug.LogWarning("Chưa có save → load level mặc định");

            isLoadingFromSave = false;
            LoadSpecificLevel(0, TravelDirection.Default);
            return;
        }

        string json = PlayerPrefs.GetString("SAVE_DATA");
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        isLoadingFromSave = true;
        savedPlayerPosition = data.playerPosition;

        LoadSpecificLevel(data.levelIndex, TravelDirection.Default);
    }

    


}