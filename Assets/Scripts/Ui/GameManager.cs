using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

public class GameManager : MonoBehaviour
{
    [Header("Save Setting")]
    [SerializeField] private int saveSlot = 0;
    [SerializeField] private bool autoSave = true;
    [SerializeField] private float autoSaveInterval = 30f;

    [Header("Debug")]
    public bool addAllItemCheat;

    private bool hasLoaded = false;
    private Coroutine autoSaveRoutine;

    private void Awake()
    {
        SaveManager.Initialize();
    }

    private IEnumerator Start()
    {
        yield return null; // đợi hệ thống khác Awake xong

        LoadGame();
        hasLoaded = true;

        if (addAllItemCheat)
            StartCheatIfNeeded();

        if (autoSave)
            autoSaveRoutine = StartCoroutine(AutoSaveLoop());
    }

    #region SAVE / LOAD

    public void SaveGame()
    {
        if (!hasLoaded)
        {
            Debug.LogWarning("Skip save: game not loaded yet.");
            return;
        }

        var c = CommonReferent.Instance;

        SaveManager.Save(
            saveSlot,
            c.playerStats,
            c.inventory,
            c.equipment,
            c.skill,
            c.playerLevel
        );
    }

    public void LoadGame()
    {
        var c = CommonReferent.Instance;

        bool loaded = SaveManager.Load(
            saveSlot,
            c.playerStats,
            c.inventory,
            c.equipment,
            c.itemDatabase,
            c.skill,
            c.playerLevel
        );

        if (loaded)
            c.playerLevelUI.RefreshUI();
    }

    #endregion

    #region AUTO SAVE

    private IEnumerator AutoSaveLoop()
    {
        while (true)
        {
            //yield return new WaitForSeconds(autoSaveInterval);
            yield return new WaitForSecondsRealtime(autoSaveInterval);
            SaveGame();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveGame();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
            SaveGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    [Button]
    public void ClearAllData()
    {
        SaveManager.ClearAll();
    }

    #endregion

    #region CHEAT

    private void StartCheatIfNeeded()
    {
        var inv = CommonReferent.Instance.inventory;

        if (!inv.IsEmpty())
            return;

        foreach (var item in CommonReferent.Instance.itemDatabase.allItems)
        {
            if (item == null) continue;

            inv.AddItem(new ItemInstance(item));
        }

        Debug.Log("Cheat: Added all items (once).");
    }

    #endregion
}
