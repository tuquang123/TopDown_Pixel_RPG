using System;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    public int Gold { get; private set; }
    public int Gems { get; private set; }
    public List<ItemData> shopItems; // Danh sách vật phẩm cửa hàng

    public event Action<int> OnGoldChanged;
    public event Action<int> OnGemsChanged;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadCurrency();
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        OnGoldChanged?.Invoke(Gold);
        SaveCurrency();
        Debug.Log($"Đã thêm {amount} vàng. Tổng vàng: {Gold}");
    }

    public void AddGems(int amount)
    {
        Gems += amount;
        OnGemsChanged?.Invoke(Gems);
        SaveCurrency();
        Debug.Log($"Đã thêm {amount} ngọc. Tổng ngọc: {Gems}");
    }

    public bool SpendGold(int amount)
    {
        if (Gold < amount)
        {
            Debug.Log($"Không đủ vàng để tiêu {amount}. Tổng vàng hiện tại: {Gold}");
            return false;
        }
        Gold -= amount;
        OnGoldChanged?.Invoke(Gold);
        SaveCurrency();
        Debug.Log($"Đã tiêu {amount} vàng. Tổng vàng: {Gold}");
        return true;
    }

    private void SaveCurrency()
    {
        PlayerPrefs.SetInt("Gold", Gold);
        PlayerPrefs.SetInt("Gems", Gems);
        PlayerPrefs.Save();
    }

    private void LoadCurrency()
    {
        Gold = PlayerPrefs.GetInt("Gold", 0);
        Gems = PlayerPrefs.GetInt("Gems", 0);
        OnGoldChanged?.Invoke(Gold);
        OnGemsChanged?.Invoke(Gems);
    }
}