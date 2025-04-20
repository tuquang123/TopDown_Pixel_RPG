using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    public int Gold { get; private set; }
    public int Gems { get; private set; }

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
    }

    public void AddGems(int amount)
    {
        Gems += amount;
        OnGemsChanged?.Invoke(Gems);
        SaveCurrency();
    }

    public bool SpendGold(int amount)
    {
        if (Gold < amount) return false;
        Gold -= amount;
        OnGoldChanged?.Invoke(Gold);
        SaveCurrency();
        return true;
    }

    private void SaveCurrency()
    {
        PlayerPrefs.SetInt("Gold", Gold);
        PlayerPrefs.SetInt("Gems", Gems);
    }

    private void LoadCurrency()
    {
        Gold = PlayerPrefs.GetInt("Gold", 0);
        Gems = PlayerPrefs.GetInt("Gems", 0);
    }
}