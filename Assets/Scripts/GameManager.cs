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
        ItemData hair = itemDatabase.GetItemByName("Hair");
        ItemData boosts = itemDatabase.GetItemByName("Boosts");
        ItemData helmet = itemDatabase.GetItemByName("Helmet");

        // Thêm item vào Inventory mà không trang bị ngay
        inventory.AddItem(sword);
        inventory.AddItem(armor);
        inventory.AddItem(hair);
        inventory.AddItem(boosts);
        inventory.AddItem(helmet);

        Debug.Log("Item đã được thêm vào Inventory.");
    }
}