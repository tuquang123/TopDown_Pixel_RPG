using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapNodeUI : MonoBehaviour
{
    [Header("Config")]
    public int levelIndex; // set tay trong Inspector

    [Header("UI")]
    public Image selected;
    public TMP_Text levelNameText; // 👈 thêm cái này

    private LevelDatabase database;

    public void Init(LevelDatabase db)
    {
        database = db;

        UpdateLevelName(); // 👈 set tên ngay khi init
    }

    public void Refresh(int currentIndex)
    {
        if (database == null)
        {
            Debug.LogError("MapNodeUI chưa được Init database");
            return;
        }

        bool isCurrent = (levelIndex == currentIndex);

        if (selected != null)
            selected.gameObject.SetActive(isCurrent);

        UpdateLevelName(); // 👈 đảm bảo luôn đúng
    }

    private void UpdateLevelName()
    {
        if (database == null || levelNameText == null) return;

        var level = database.GetLevel(levelIndex);
        if (level == null) return;

        levelNameText.text = level.levelName;
    }

    public LevelDatabase.LevelEntry GetLevelData()
    {
        if (database == null) return null;

        return database.GetLevel(levelIndex);
    }
}