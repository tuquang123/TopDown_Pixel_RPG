using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GachaPopup : BasePopup
{
    [Header("Gacha Data")]
    public GachaItemData gachaData;

    [Header("Result UI")]
    public GameObject resultPanel;
    public Image itemIcon;
    public TMP_Text itemNameText;
    [Header("Result Grid")]
    public Transform resultContainer;
    public GachaItemUI gachaItemPrefab;
    [Header("Info Panel")]
    public GameObject infoPanel;
    public Transform infoContainer;

    private bool isRolling = false;

    public int gemCost = 1;

    // ===================== BUTTON EVENTS =====================

    public void OnClickRollX1()
    {
        if (!isRolling) Roll();
    }

    public void OnClickRollX5()
    {
        if (!isRolling) RollX5();
    }

    public void OnClickInfo()
    {
        ClearInfo();
        infoPanel.SetActive(true);
        infoPanel.transform.localScale = Vector3.zero;
        infoPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        foreach (var g in gachaData.items)
        {
            var ui = Instantiate(gachaItemPrefab, infoContainer);
            ui.Setup(new ItemInstance(g.item));
        }
    }

    public void OnCloseInfo()
    {
        infoPanel.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
        {
            infoPanel.SetActive(false);
            ClearInfo();
        });
    }

    public void OnCloseClick()
    {
        UIManager.Instance.HidePopupByType(PopupType.Gacha);
    }

    public void OnConfirmClick()
    {
        ClearResult();
    }

    // ===================== LOGIC =====================

    private void Start()
    {
        Show();
        ClearResult();
    }

    private ItemInstance RollOne()
    {
        if (gachaData == null || gachaData.items.Count == 0)
            return null;

        float totalRate = 0f;
        foreach (var g in gachaData.items)
            totalRate += g.rate;

        float rand = Random.Range(0, totalRate);
        float current = 0f;

        foreach (var g in gachaData.items)
        {
            current += g.rate;
            if (rand <= current)
                return new ItemInstance(g.item);
        }

        return null;
    }

    // ===================== ROLL X1 =====================

    public void Roll()
    {
        if (!CurrencyManager.Instance.SpendGems(gemCost))
        {
            GameEvents.OnShowToast.Raise("Not enough Gems");
            return;
        }

        isRolling = true;

        if (infoPanel != null)
            infoPanel.SetActive(false);

        ClearRollResult();

        var item = RollOne();
        if (item == null)
        {
            isRolling = false;
            return;
        }

        Inventory.Instance.AddItem(item);
        QuestManager.Instance?.ReportProgressByObjectiveName("Gacha", 1);
        ShowItemWithEffect(item, 0);

        DOVirtual.DelayedCall(0.8f, () => { isRolling = false; });
    }

    // ===================== ROLL X5 =====================

    public void RollX5()
    {
        int cost = gemCost * 5;

        if (!CurrencyManager.Instance.SpendGems(cost))
        {
            GameEvents.OnShowToast.Raise("Not enough Gems");
            return;
        }

        isRolling = true;

        if (infoPanel != null)
            infoPanel.SetActive(false);

        ClearRollResult();

        for (int i = 0; i < 5; i++)
        {
            var item = RollOne();
            if (item == null) continue;

            Inventory.Instance.AddItem(item);
            QuestManager.Instance?.ReportProgressByObjectiveName("Gacha", 1);
            ShowItemWithEffect(item, i);
        }

        DOVirtual.DelayedCall(1.5f, () => { isRolling = false; });
    }

    // ===================== HIỆU ỨNG HIỂN THỊ ITEM - POP IN ELASTIC =====================

    void ShowItemWithEffect(ItemInstance item, int itemIndex)
    {
        resultPanel.SetActive(true);

        var ui = Instantiate(gachaItemPrefab, resultContainer);
        ui.Setup(item);

        RectTransform rt = ui.GetComponent<RectTransform>();
        if (rt != null)
        {
            // Reset scale về 0
            rt.localScale = Vector3.zero;

            // Tính delay để tạo hiệu ứng xuất hiện tuần tự (stagger)
            float delay = itemIndex * 0.15f;

            // 🔹 POP-IN ELASTIC: Item phóng to lên rồi co lại đàn hồi
            rt.DOScale(Vector3.one, 0.6f)
                .SetDelay(delay)
                .SetEase(Ease.OutElastic);
        }
    }

    // ===================== HELPER FUNCTIONS =====================

    void ClearResult()
    {
        foreach (Transform child in resultContainer)
            Destroy(child.gameObject);

        resultPanel.SetActive(false);
    }

    void ClearRollResult()
    {
        foreach (Transform child in resultContainer)
            Destroy(child.gameObject);

        resultPanel.SetActive(false);
    }

    void ClearInfo()
    {
        foreach (Transform child in infoContainer)
            Destroy(child.gameObject);
    }

    void ShowResult(ItemData item)
    {
        resultPanel.SetActive(true);
        itemIcon.sprite = item.icon;
        itemNameText.text = item.itemName;
    }

    void ShowItem(ItemInstance item)
    {
        resultPanel.SetActive(true);

        var ui = Instantiate(gachaItemPrefab, resultContainer);
        ui.Setup(item);
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
    
}
