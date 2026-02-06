using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GachaPopup : BasePopup
{
    [Header("Gacha Data")]
    public GachaItemData gachaData;

    [Header("Result UI")]
    public GameObject resultPanel;
    public Image itemIcon;
    public TMP_Text itemNameText;
    [Header("Result Grid")]
    public Transform resultContainer;        // GridLayoutGroup
    public GachaItemUI gachaItemPrefab;      // prefab item

    public void OnClickRollX1()
    {
        Roll();
    }

    public void OnClickRollX5()
    {
        RollX5();
    }

    private ItemInstance lastRolledItem;


    private void Start()
    {
        Show();
        ClearResult();
    }

    public int gemCost = 1;

  

    void ShowResult(ItemData item)
    {
        resultPanel.SetActive(true);
        itemIcon.sprite = item.icon;
        itemNameText.text = item.itemName;
    }

    void ClearResult()
    {
        foreach (Transform child in resultContainer)
            Destroy(child.gameObject);

        resultPanel.SetActive(false);
    }
    void ShowItem(ItemInstance item)
    {
        resultPanel.SetActive(true);

        var ui = Instantiate(gachaItemPrefab, resultContainer);
        ui.Setup(item);
    }

    // ===================== BUTTON =====================

    // 🔹 NHẬN ITEM – chỉ tắt panel kết quả
   
   
    // 🔹 THOÁT GACHA – tắt toàn bộ popup + blur
    public void OnCloseClick()
    {
        UIManager.Instance.HidePopupByType(PopupType.Gacha);
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

    public void Roll()
    {
        if (!CurrencyManager.Instance.SpendGems(gemCost))
        {
            GameEvents.OnShowToast.Raise("Không đủ Gem");
            return;
        }

        ClearResult();

        var item = RollOne();
        if (item == null) return;

        Inventory.Instance.AddItem(item);
        ShowItem(item);
    }

    public void RollX5()
    {
        int cost = gemCost * 5;

        if (!CurrencyManager.Instance.SpendGems(cost))
        {
            GameEvents.OnShowToast.Raise("Không đủ Gem");
            return;
        }

        ClearResult();

        for (int i = 0; i < 5; i++)
        {
            var item = RollOne();
            if (item == null) continue;

            Inventory.Instance.AddItem(item);
            ShowItem(item);
        }
    }
    public void OnConfirmClick()
    {
        ClearResult();
    }


}