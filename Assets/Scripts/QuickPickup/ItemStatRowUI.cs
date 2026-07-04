using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemStatRowUI : MonoBehaviour
{
    public Image statIcon;
    public TMP_Text statLabel;
    public TMP_Text statValue;

    public void Setup(string label, ItemStatBonus bonus, Sprite icon = null)
    {
        if (statIcon != null)
        {
            statIcon.sprite = icon;
            statIcon.gameObject.SetActive(icon != null);
        }

        statLabel.text = label;
        statValue.text = bonus.ToString();
    }
}