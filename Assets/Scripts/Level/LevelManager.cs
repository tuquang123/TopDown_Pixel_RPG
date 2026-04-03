using System;
using UnityEngine;
using System.Collections;

public class LevelManager : Singleton<LevelManager>
{
    public enum TravelDirection { Forward, Backward, Default }

    public LevelDatabase levelDatabase;
    private GameObject player;

    private int currentLevel = 0;
    private GameObject currentLevelInstance;
    private bool isLoadingFromSave = false;
    private Vector3 savedPlayerPosition;
    [SerializeField] private ScreenFader screenFader;

    public int CurrentLevel => currentLevel;

    void Start()
    {
        levelDatabase = CommonReferent.Instance.levelDatabase;

        // Instantiate player nếu chưa có
        if (GameObject.FindWithTag("Player") == null)
        {
            player = Instantiate(CommonReferent.Instance.playerPrefab);
            player.tag = "Player";
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
        StartCoroutine(LoadLevelCoroutine(index, direction));
    }

    public void NextLevel()
    {
        currentLevel++;
        if (currentLevel >= levelDatabase.TotalLevels) currentLevel = 0;
        StartCoroutine(LoadLevelCoroutine(currentLevel, TravelDirection.Forward));
    }

    public void PreviousLevel()
    {
        currentLevel--;
        if (currentLevel < 0) currentLevel = levelDatabase.TotalLevels - 1;
        StartCoroutine(LoadLevelCoroutine(currentLevel, TravelDirection.Backward));
    }
    
    public void ResetLevel()
    {
        StartCoroutine(LoadLevelCoroutine(currentLevel, TravelDirection.Default));
    }

    private void PositionAlliesAroundPlayer(Vector3 center)
    {
        if (AllyManager.Instance == null) return;

        foreach (var ally in AllyManager.Instance.GetAllies())
        {
            if (ally == null) continue;
            Vector2 offset = AllyManager.Instance.GetOffsetPosition(ally);
            ally.transform.position = center + (Vector3)offset;
        }
    }

    void UpdateMapUI()
    {
        MapPopup popup = FindObjectOfType<MapPopup>();
        if (popup != null)
            popup.RefreshMap();
    }

    // ====================== SAVE & LOAD ======================
    [System.Serializable]
    private class SaveData
    {
        public int levelIndex;
        public Vector3 playerPosition;
    }

    public void SaveGame()
    {
        if (player == null) return;

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
            Debug.LogWarning("Chưa có save → load level 0");
            isLoadingFromSave = false;
            LoadSpecificLevel(0, TravelDirection.Default);
            return;
        }

        string json = PlayerPrefs.GetString("SAVE_DATA");
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        isLoadingFromSave = true;
        savedPlayerPosition = data.playerPosition;

        Debug.Log($"Load save: level {data.levelIndex}");
        LoadSpecificLevel(data.levelIndex, TravelDirection.Default);
    }
    
   private IEnumerator LoadLevelCoroutine(int index, TravelDirection direction)
{
    Debug.Log($"[LevelManager] Bắt đầu load level {index}");
    
    // === 0. FADE IN ===
    bool fadeDone = false;
    if (screenFader != null)
    {
        screenFader.FadeIn(0.3f, () => fadeDone = true);
        yield return new WaitUntil(() => fadeDone);
    }

    // 🔥 CLEAR ARROW TRƯỚC KHI LOAD (TRÁNH GIỮ TARGET CŨ)
    if (QuestManager.Instance != null)
    {
        QuestManager.Instance.ForceClearArrow();
    }

    // === 1. DESTROY LEVEL CŨ ===
    if (currentLevelInstance != null)
    {
        Debug.Log("[LevelManager] Destroying old level...");
        EnemyTracker.Instance?.ClearAllEnemies();
        ObjectPooler.Instance?.ClearAllPools();
        Destroy(currentLevelInstance);
        currentLevelInstance = null;

        yield return null; // 🔥 QUAN TRỌNG
    }

    // === 2. LOAD LEVEL MỚI ===
    var levelData = levelDatabase.GetLevel(index);
    if (levelData == null)
    {
        Debug.LogError($"[LevelManager] LevelData {index} không tìm thấy!");
        yield break;
    }

    Debug.Log($"[LevelManager] Instantiating level: {levelData.levelName}");
    currentLevelInstance = Instantiate(levelData.levelPrefab);

    // === 3. SET PLAYER POSITION ===
    Vector3 targetPos = direction switch
    {
        TravelDirection.Forward => levelData.entryFromPreviousLevel,
        TravelDirection.Backward => levelData.entryFromNextLevel,
        _ => CommonReferent.Instance.defaultEntryPosition
    };

    Vector3 finalPos = isLoadingFromSave ? savedPlayerPosition : targetPos;

    if (player == null)
        player = GameObject.FindWithTag("Player");

    if (player != null)
        player.transform.position = finalPos;
    else
        Debug.LogError("[LevelManager] Player không tồn tại!");

    PositionAlliesAroundPlayer(finalPos);
    UpdateMapUI();
    isLoadingFromSave = false;

    // === 4. SPAWN ENEMY / NPC ===
    Debug.Log("[LevelManager] Spawning enemies & NPCs...");
    foreach (var sp in currentLevelInstance.GetComponentsInChildren<SpawnPoint>())
    {
        sp.Spawn(CommonReferent.Instance.enemyLevelDatabase);
    }

    // 🔥 ĐỢI ENEMY REGISTER XONG (CỰC QUAN TRỌNG)
    yield return new WaitForSeconds(0.1f);

    // 🔥 UPDATE ARROW SAU KHI ENEMY SẴN SÀNG
    if (QuestManager.Instance != null)
    {
        QuestManager.Instance.UpdateArrow();
    }

    Debug.Log($"[LevelManager] ĐÃ LOAD XONG LEVEL {index}: {levelData.levelName}");

    // === 5. FADE OUT ===
    if (screenFader != null)
    {
        screenFader.FadeOut(0.3f);
    }
}
}