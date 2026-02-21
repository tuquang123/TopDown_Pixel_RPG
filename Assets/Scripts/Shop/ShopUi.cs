using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopUI : BasePopup
{
    [Header("UI")]
    [SerializeField] private ShopItemUI itemUIPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private ShopDetailPopup detailPopup;

    [Header("Filter Buttons")]
    [SerializeField] private List<FilterButtonUI> filterButtons = new();

    [Header("Data")]
    Inventory playerInventory;
    public List<ItemData> allShopItems = new();
  
    // ================= INTERNAL =================
    private readonly List<ShopItemUI> activeUIs = new();
    private readonly Queue<ShopItemUI> pool = new();

    private ItemType? currentFilterType = null;
    private FilterButtonUI currentFilter;
    private bool isBuilt = false;
    
    public ShopDetailPopup DetailPopupUI => detailPopup;
    public Inventory PlayerInventory => playerInventory;

    #region SHOW / HIDE

    public override void Show()
    {
        base.Show();
        detailPopup?.Setup(this);
        playerInventory = CommonReferent.Instance.playerPrefab.GetComponent<Inventory>();
        
        if (!isBuilt)
        {
            isBuilt = true;
            SelectFilter(filterButtons[1]);
            ApplyFilter(ItemType.Weapon);
        }
        else
        {
            ApplyFilter(currentFilterType);
        }
    }
    
    public void Close()
    {
            UIManager.Instance.HidePopupByType(PopupType.Shop);
    }

    #endregion

    #region FILTER BUTTONS
    public void OnFilterConsumable(FilterButtonUI btn)
    {
        SelectFilter(btn);
        ApplyFilter(ItemType.Consumable);
    }
    private void SelectFilter(FilterButtonUI btn)
    {
        if (currentFilter == btn)
            return;

        if (currentFilter != null)
            currentFilter.SetSelected(false);

        currentFilter = btn;
        currentFilter.SetSelected(true);
    }

  
    public void OnFilterWeapon(FilterButtonUI btn)
    {
        SelectFilter(btn);
        ApplyFilter(ItemType.Weapon);
    }

    public void OnFilterHelmet(FilterButtonUI btn)
    {
        SelectFilter(btn);
        ApplyFilter(ItemType.Helmet);
    }

    public void OnFilterBoots(FilterButtonUI btn)
    {
        SelectFilter(btn);
        ApplyFilter(ItemType.Boots);
    }

    public void OnFilterArmor(FilterButtonUI btn)
    {
        SelectFilter(btn);
        ApplyFilter(ItemType.SpecialArmor);
    }

    public void OnFilterBody(FilterButtonUI btn)
    {
        SelectFilter(btn);
        ApplyFilter(ItemType.Clother);
    }

    public void OnFilterPet(FilterButtonUI btn)
    {
        SelectFilter(btn);
        ApplyFilter(ItemType.Horse);
    }

    public void OnFilterHair(FilterButtonUI btn)
    {
        SelectFilter(btn);
        ApplyFilter(ItemType.Hair);
    }

    public void OnFilterCloak(FilterButtonUI btn)
    {
        SelectFilter(btn);
        ApplyFilter(ItemType.Cloak);
    }

    #endregion

    #region FILTER LOGIC

    private void ApplyFilter(ItemType? type)
    {
        currentFilterType = type;

        var filteredItems = ItemFilter.FilterByType(allShopItems, type)
            .OrderBy(i => i.tier)
            .ThenBy(i => i.price)
            .Where(i => type == ItemType.Consumable || !IsItemOwned(i))
            .ToList();

        BuildShop(filteredItems);
    }

    #endregion

    #region SHOP BUILD (POOL BASED)

    public void BuildShop(List<ItemData> items)
    {
        ReleaseAll();

        if (items == null || items.Count == 0)
            return;

        for (int i = 0; i < items.Count; i++)
        {
            var data = items[i];
            if (data == null)
                continue;

            var ui = GetUI();
            ui.transform.SetParent(contentParent, false);
            ui.gameObject.SetActive(true);

            var instance = new ItemInstance(data);
            ui.Setup(instance, this);

            activeUIs.Add(ui);
        }
    }

    private void ReleaseAll()
    {
        for (int i = 0; i < activeUIs.Count; i++)
        {
            var ui = activeUIs[i];
            ui.gameObject.SetActive(false);
            pool.Enqueue(ui);
        }
        activeUIs.Clear();
    }

    private ShopItemUI GetUI()
    {
        if (pool.Count > 0)
            return pool.Dequeue();

        var ui = Instantiate(itemUIPrefab, contentParent);
        ui.gameObject.SetActive(false);
        return ui;
    }

    #endregion

    #region BUY

    public void BuyItem(ItemInstance instance)
    {
        if (instance == null || instance.itemData == null)
            return;

        detailPopup.Hide();

        var data = instance.itemData;

        if (data.itemType != ItemType.Consumable && IsItemOwned(data))
            return;

        if (!CurrencyManager.Instance.SpendGold(data.price))
        {
            GameEvents.OnShowToast.Raise("Gold not enough!");
            return;
        }

        playerInventory.AddItem(new ItemInstance(data));

        ApplyFilter(currentFilterType);

        GameEvents.OnShowToast.Raise("Success purchase Item!");
    }
    #endregion

    #region UTIL

    private bool IsItemOwned(ItemData data)
    {
        return playerInventory.items.Any(i => i.itemData.itemID == data.itemID);
    }

    #endregion
    [System.Serializable]
    public class ConsumableStack
    {
        public ItemData itemData;
        public int amount;

        public ConsumableStack(ItemData data, int amount)
        {
            itemData = data;
            this.amount = amount;
        }
    }
}
