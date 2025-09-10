using UnityEngine;
using UnityEngine.UI;

public class ItemIconHandler : MonoBehaviour
{
    [Header("Icons")]
    public Image itemIcon;
    public Image itemIconBody;
    public Image itemIconLeft;
    public Image itemIconRight;
    public Image itemIconLegLeft;
    public Image itemIconLegRight;

    public void SetupIcons(ItemInstance data)
    {
        HideAllIcons();

        switch (data.itemData.itemType)
        {
            case ItemType.SpecialArmor:
            case ItemType.Clother:
                SetIcon(itemIconBody, data.itemData.icon);
                SetIcon(itemIconLeft, data.itemData.iconLeft);
                SetIcon(itemIconRight, data.itemData.iconRight);
                break;

            case ItemType.Boots:
                SetIcon(itemIconLegLeft, data.itemData.iconLeft);
                SetIcon(itemIconLegRight, data.itemData.iconRight);
                break;

            default:
                SetIcon(itemIcon, data.itemData.icon);
                break;
        }
    }

    public void HideAllIcons()
    {
        itemIcon.gameObject.SetActive(false);
        itemIconBody.gameObject.SetActive(false);
        itemIconLeft.gameObject.SetActive(false);
        itemIconRight.gameObject.SetActive(false);
        itemIconLegLeft.gameObject.SetActive(false);
        itemIconLegRight.gameObject.SetActive(false);
    }

    private void SetIcon(Image img, Sprite sprite)
    {
        if (sprite != null)
        {
            img.sprite = sprite;
            img.gameObject.SetActive(true);
        }
    }
}
