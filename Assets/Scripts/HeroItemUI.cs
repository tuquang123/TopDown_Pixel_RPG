using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroItemUI : MonoBehaviour
{
    public TMP_Text nameText;
    public Image icon;
    private Hero heroData;
    private System.Action<Hero> onDeployCallback;

    public void Setup(Hero hero, System.Action<Hero> onDeploy)
    {
        heroData = hero;
        nameText.text = hero.data.name;
        icon.sprite = hero.data.icon;
        onDeployCallback = onDeploy;
    }

    public void OnClickDeploy()
    {
        onDeployCallback?.Invoke(heroData);
    }
}