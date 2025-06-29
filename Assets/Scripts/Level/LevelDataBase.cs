using UnityEngine;

[CreateAssetMenu(menuName = "GameData/Level Database", fileName = "LevelDatabase")]
public class LevelDatabase : ScriptableObject
{
    [System.Serializable]
    public class LevelEntry
    {
        public string levelName;
        public GameObject levelPrefab;
        public Vector3 entryFromNextLevel;   
        public Vector3 entryFromPreviousLevel;
    }

    public LevelEntry[] levels;

    public LevelEntry GetLevel(int index)
    {
        if (index < 0 || index >= levels.Length)
        {
            Debug.LogWarning($"Không có level ở index: {index}");
            return null;
        }

        return levels[index];
    }

    public int TotalLevels => levels.Length;
}
