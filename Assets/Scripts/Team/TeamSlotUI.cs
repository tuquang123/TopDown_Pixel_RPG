using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeamSlotUI : MonoBehaviour
{
    public TMP_Text nameText;
    public Image icon;
    public Button removeButton;

    private int slotIndex;
    private System.Action<int> onRemoveCallback;

    public void Setup(int index, System.Action<int> onRemove)
    {
        slotIndex = index;
        onRemoveCallback = onRemove;
        removeButton.onClick.AddListener(() => onRemoveCallback?.Invoke(slotIndex));
    }

    public void UpdateSlot(Hero hero)
    {
        if (hero != null)
        {
            nameText.text = hero.data.name;
            icon.sprite = hero.data.icon;
            icon.gameObject.SetActive(true);
            removeButton.gameObject.SetActive(true);
        }
        else
        {
            nameText.text = "Empty";
            icon.gameObject.SetActive(false);
            removeButton.gameObject.SetActive(false);
        }
    }
    public void ClearSlot()
    {
        icon.sprite = null;
        nameText.text = "Empty";
    }

}