using UnityEngine;
using UnityEngine.UI;

public class CurrencyUI : MonoBehaviour
{
    [SerializeField] private Text goldText;
    [SerializeField] private Text gemsText;

    void Start()
    {
        CurrencyManager.Instance.OnGoldChanged += UpdateGold;
        CurrencyManager.Instance.OnGemsChanged += UpdateGems;

        UpdateGold(CurrencyManager.Instance.Gold);
        UpdateGems(CurrencyManager.Instance.Gems);
    }

    void UpdateGold(int gold) => goldText.text = $"Gold: {gold}";
    void UpdateGems(int gems) => gemsText.text = $"Gems: {gems}";
}