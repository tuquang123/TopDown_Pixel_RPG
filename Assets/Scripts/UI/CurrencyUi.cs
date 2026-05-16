using TMPro;
using UnityEngine;

public class CurrencyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI gemsText;

    void Start()
    {
        CurrencyManager.Instance.OnGoldChanged += UpdateGold;
        CurrencyManager.Instance.OnGemsChanged += UpdateGems;

        UpdateGold(CurrencyManager.Instance.Gold);
        UpdateGems(CurrencyManager.Instance.Gems);
    }

    void UpdateGold(int gold) => goldText.text = CurrencyManager.FormatGold(gold);
    void UpdateGems(int gems) => gemsText.text = $"{gems}";
}