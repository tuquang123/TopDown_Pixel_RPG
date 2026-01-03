using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public bool allAllItem;
    
    private bool hasLoaded = false;
    
    private IEnumerator Start()
    {
        yield return null;

        LoadGame();
        hasLoaded = true;

        if (allAllItem)
            StartCheatIfNeeded();
    }
    
    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveGame(); // Auto save khi minimize hoặc thoát app
    }

    private void OnApplicationQuit()
    {
#if UNITY_EDITOR
        SaveGame();
        return;
#endif
        SaveGame();
    }


    public void SaveGame()
    {
        if (!hasLoaded)
        {
            Debug.LogWarning("[GameManager] Skip save: game not loaded yet");
            return;
        }

        SaveManager.Save(
            CommonReferent.Instance.playerStats,
            CommonReferent.Instance.inventory,
            CommonReferent.Instance.equipment,
            CommonReferent.Instance.skill,
            CommonReferent.Instance.playerLevel
        );
    }


    public void LoadGame()
    {
        SaveManager.Load(CommonReferent.Instance.playerStats,
            CommonReferent.Instance.inventory,
            CommonReferent.Instance.equipment,
            CommonReferent.Instance.itemDatabase,
            CommonReferent.Instance.skill,
            CommonReferent.Instance.playerLevel);
        
        CommonReferent.Instance.playerLevelUI.RefreshUI(); 

    }

    [Button("Clear Save Data")]
    private void ClearSave()
    {
        SaveManager.Clear();
    }
    

    // Cheat
    private void AddAllItemsToInventory()
    {
        foreach (var item in CommonReferent.Instance.itemDatabase.allItems)
        {
            if (item != null)
            {
                ItemInstance itemInstance = new ItemInstance(item);
                CommonReferent.Instance.inventory.AddItem(itemInstance);
            }
        }
    }
    private void StartCheatIfNeeded()
    {
        if (allAllItem)
        {
            // Chỉ cheat nếu inventory rỗng
            if (CommonReferent.Instance.inventory.IsEmpty())
            {
                AddAllItemsToInventory();
                Debug.Log("Cheat: Added all items to inventory (only once).");
            }
            else
            {
                Debug.Log("Inventory already has items, skip cheat.");
            }
        }
    }
    

}
