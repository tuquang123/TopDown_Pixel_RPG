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
        itemInstance = instance;

        var data = instance.itemData;

        // icon + frame
        icon.SetupIcons(instance);

        nameText.text = data.itemName;
        tierText.text = data.tier.ToString();
        backgroundImage.sprite =
            CommonReferent.Instance.itemTierColorConfig
                .GetBackground(data.tier);
    }
}