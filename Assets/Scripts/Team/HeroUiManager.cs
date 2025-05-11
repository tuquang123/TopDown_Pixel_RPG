using UnityEngine;

public class HeroUIManager : MonoBehaviour
{
    public HeroItemUI heroItemPrefab;
    public Transform heroListRoot;

    public TeamSlotUI[] teamSlots;

    public HeroManager heroManager;

    void Start()
    {
        InitHeroListUI();
        InitTeamSlotUI();
        UpdateTeamSlotUI();
    }

    void InitHeroListUI()
    {
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
        Debug.Log("Deloy : " + hero.data.id);
        
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