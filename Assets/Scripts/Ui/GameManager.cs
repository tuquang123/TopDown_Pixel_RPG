using UnityEngine;

public class GameManager : MonoBehaviour
{
    public ItemDatabase itemDatabase;
    public Inventory inventory;

    private void Start()
    {
        AddAllItemsToInventory();
    }
    private void AddAllItemsToInventory()
    {
        foreach (var item in itemDatabase.allItems)
        {
            if (item != null)
            {
                inventory.AddItem(item);
            }
        }
    }
}