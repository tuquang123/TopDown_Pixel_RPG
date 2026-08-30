using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaItemUI : MonoBehaviour
{
    public ItemIconHandler icon;
    public Image backgroundImage;
    public TMP_Text nameText;
    public TMP_Text tierText;
   

    private ItemInstance itemInstance;

    public void Setup(ItemInstance instance)
    {
        if (instance == null || instance.itemData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        itemInstance = instance;

        var data = instance.itemData;

        // icon + frame
        if (icon != null)
            icon.SetupIcons(instance);

        if (nameText != null)
            nameText.text = data.itemName;

        if (tierText != null)
            tierText.text = ItemUtility.GetLocalizedTier(data.tier);

        if (backgroundImage != null && CommonReferent.Instance?.itemTierColorConfig != null)
            backgroundImage.sprite = CommonReferent.Instance.itemTierColorConfig.GetBackground(data.tier);
    }
}
