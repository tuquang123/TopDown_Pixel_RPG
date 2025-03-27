using UnityEngine;

public class GameManager : MonoBehaviour
{
    public PlayerStats playerStats;
    public Equipment equipment;
    public ItemDatabase itemDatabase;
    public Inventory inventory;

    private void Start()
    {
        // Lấy item từ ItemDatabase
        ItemData sword = itemDatabase.GetItemByName("Sword");
        ItemData armor = itemDatabase.GetItemByName("Armor");

        // Thêm item vào Inventory mà không trang bị ngay
        inventory.AddItem(sword);
        inventory.AddItem(armor);

        Debug.Log("Item đã được thêm vào Inventory.");
    }
}