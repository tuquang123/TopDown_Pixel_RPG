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

    public LevelDatabase levelDatabase;
    GameObject player;

    private int currentLevel = 0;
    public GameObject currentLevelInstance;
    private bool isLoadingFromSave = false;
    private Vector3 savedPlayerPosition;
    [SerializeField] private ScreenFader screenFader;
    public int CurrentLevel => currentLevel;

    void Start()
    {
        levelDatabase = CommonReferent.Instance.levelDatabase;
    
        // Nếu player chưa tồn tại trong scene
        if (GameObject.FindWithTag("Player") == null)
        {
            player = Instantiate(CommonReferent.Instance.playerPrefab);
            player.tag = "Player";  // Đảm bảo tag
        }
        else
        {
            player = GameObject.FindWithTag("Player");
        }

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

    private void LoadLevel(int index, TravelDirection direction = TravelDirection.Default)
    {
        if (screenFader == null)
        {
            Debug.LogError("ScreenFader NULL → load level trực tiếp");
            InternalLoadLevel(index, direction);
            return;
        }

        screenFader.FadeIn(0.5f, () =>
        {
            InternalLoadLevel(index, direction);
            screenFader.FadeOut(0.5f);
        });
    }
    
   private void InternalLoadLevel(int index, TravelDirection direction)
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

    // Đảm bảo player tồn tại và set vị trí
    if (player == null)
    {
        player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player không tồn tại trong scene khi load level!");
            return;
        }
    }
    player.transform.position = finalPos;

    PositionAlliesAroundPlayer(finalPos);

    UpdateMapUI();
    isLoadingFromSave = false;

    // Spawn enemies/NPC (tạo ra các NPC mới trong level)
    foreach (var sp in currentLevelInstance.GetComponentsInChildren<SpawnPoint>())
    {
        var levelDB = CommonReferent.Instance.enemyLevelDatabase;
        sp.Spawn(levelDB);
    }

    Debug.Log($"Đã load level {index}: {levelData.levelName}");

    // ★★★ CÁC BƯỚC FIX SAU KHI LOAD LEVEL MỚI ★★★
    
    // 1. Buộc update Quest Arrow (mũi tên chỉ NPC)
    QuestManager.Instance?.UpdateArrow();

    // 2. Buộc update tất cả NPCDialogueTrigger (icon quest, currentQuest, tương tác)
    var allNpcs = FindObjectsOfType<NPCDialogueTrigger>();
    foreach (var npc in allNpcs)
    {
        npc.UpdateCurrentQuest();           // cập nhật icon available/turn-in
        npc.Start();                        // gọi lại Start() để refresh player reference + các thứ khác nếu cần
    }

    // Optional: Nếu bạn có quest UI cần refresh toàn bộ
    if (QuestManager.Instance.questUI != null && QuestManager.Instance.activeQuests.Count > 0)
    {
        foreach (var qp in QuestManager.Instance.activeQuests)
        {
            QuestManager.Instance.questUI.UpdateQuestProgress(qp, qp.state == QuestState.Completed);
        }
    }
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
        public string questSaveJson;
    }
    public void SaveGame()
    {
        SaveData data = new SaveData
        {
            levelIndex = currentLevel,
            playerPosition = player.transform.position,
            questSaveJson = JsonUtility.ToJson(QuestManager.Instance.ToData())  // lưu quest
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SAVE_DATA", json);
        PlayerPrefs.Save();
        Debug.Log("Save game thành công (bao gồm quest)");
    }
    
    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey("SAVE_DATA"))
        {
            // ... load mặc định
            return;
        }

        string json = PlayerPrefs.GetString("SAVE_DATA");
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        isLoadingFromSave = true;
        savedPlayerPosition = data.playerPosition;

        // Load quest trước khi load level
        if (!string.IsNullOrEmpty(data.questSaveJson))
        {
            QuestSaveData questData = JsonUtility.FromJson<QuestSaveData>(data.questSaveJson);
            QuestManager.Instance.FromData(questData, QuestManager.Instance.questDatabase);
        }

        LoadSpecificLevel(data.levelIndex, TravelDirection.Default);
    }
    void UpdateMapUI()
    {
        MapPopup popup = FindObjectOfType<MapPopup>();

        if (popup != null)
        {
            popup.RefreshMap();
        }
    }
    


}