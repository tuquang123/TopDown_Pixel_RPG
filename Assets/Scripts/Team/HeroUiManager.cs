using System.Linq;
using UnityEngine;

public class HeroUI : BasePopup
{
    public HeroItemUI heroItemPrefab;
    public Transform heroListRoot;

    public TeamSlotUI[] teamSlots;

    public HeroManager heroManager;

    public override void Show()
    {
        base.Show();
       
        heroManager.InitData();
        InitHeroListUI();
        InitTeamSlotUI();
        UpdateTeamSlotUI();
    }

    public override void Hide()
    {
        base.Hide();
        // Nếu muốn clear UI hoặc reset về mặc định, làm tại đây
    }

    void InitHeroListUI()
    {
        foreach (Transform child in heroListRoot)
        {
            Destroy(child.gameObject);
        }

        foreach (var hero in heroManager.allHeroes)
        {
            var item = Instantiate(heroItemPrefab, heroListRoot);
            item.Setup(hero, OnClickDeploy);
        }
    }

    void InitTeamSlotUI()
    {
        for (int i = 0; i < teamSlots.Length; i++)
        {
            teamSlots[i].Setup(i, OnClickRemove);
        }
    }

    void UpdateTeamSlotUI()
    {
        for (int i = 0; i < teamSlots.Length; i++)
        {
            var hero = heroManager.battleTeam.teamSlots[i];
            teamSlots[i].UpdateSlot(hero);
        }
    }

    void OnClickDeploy(Hero hero)
    {
        if (heroManager.battleTeam.IsHeroInTeam(hero))
        {
            Debug.Log("Hero này đã có trong đội hình!");
            return;
        }

        int emptySlot = heroManager.battleTeam.GetFirstEmptySlot();
        if (emptySlot == -1)
        {
            Debug.Log("Đội hình đã đầy!");
            return;
        }

        GameEvents.OnDeloyTeamAssist.Raise($"{hero.data.id}|add");
        Debug.Log("Deploy: " + hero.data.id);

        heroManager.battleTeam.AddHeroToSlot(hero, emptySlot);
        UpdateTeamSlotUI();
    }

    void OnClickRemove(int slotIndex)
    {
        var hero = heroManager.battleTeam.teamSlots[slotIndex];
        if (hero != null)
        {
            GameEvents.OnDeloyTeamAssist.Raise($"{hero.data.id}|remove");
        }

        heroManager.battleTeam.RemoveHeroFromSlot(slotIndex);
        UpdateTeamSlotUI();
    }
}
