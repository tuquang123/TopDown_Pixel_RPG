using System;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    public int Gold { get; set; }
    public int Gems { get; private set; }
    public List<ItemData> shopItems;

    public event Action<int> OnGoldChanged;
    public event Action<int> OnGemsChanged;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadCurrency();
    }

    public static string FormatGold(long value)
    {
        if (value >= 1000000)
            return $"{value / 1000000f:0.#}M";
        if (value >= 1000)
            return $"{value / 1000f:0.#}K";
        return value.ToString();
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        OnGoldChanged?.Invoke(Gold);
        QuestManager.Instance?.ReportProgressByObjectiveName("Gold", amount);
        SaveCurrency();
        Debug.Log($"Added {amount} Gold. Total Gold: {Gold}");
    }

    public void AddGems(int amount)
    {
        Gems += amount;
        OnGemsChanged?.Invoke(Gems);
        QuestManager.Instance?.ReportObjectiveValue("Gem", Gems);
        SaveCurrency();
        Debug.Log($"Added {amount} Gems. Total Gems: {Gems}");
    }

    public bool SpendGold(int amount)
    {
        if (Gold < amount)
        {
            Debug.Log($"Not enough Gold to spend {amount}. Current Gold: {Gold}");
            return false;
        }
        Gold -= amount;
        OnGoldChanged?.Invoke(Gold);
        SaveCurrency();
        Debug.Log($"Spent {amount} Gold. Total Gold: {Gold}");
        return true;
    }

    public bool SpendGems(int amount)
    {
        if (Gems < amount)
        {
            Debug.Log($"Not enough Gems to spend {amount}. Current Gems: {Gems}");
            return false;
        }
        Gems -= amount;
        OnGemsChanged?.Invoke(Gems);
        SaveCurrency();
        Debug.Log($"Spent {amount} Gems. Total Gems: {Gems}");
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
