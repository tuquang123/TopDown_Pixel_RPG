using UnityEngine;

[System.Serializable]
public class AllySlot
{
    public string heroId;
    public GameObject slotObject;
}

public class TeamAssistHandler : MonoBehaviour, IGameEventListener<string>
{
    [SerializeField] private AllySlot[] allySlots;
    //[SerializeField] private HeroManager heroManager;

    private void OnEnable()
    {
        GameEvents.OnDeloyTeamAssist.RegisterListener(this);
    }

    private void OnDisable()
    {
        GameEvents.OnDeloyTeamAssist.UnregisterListener(this);
    }

    public void OnEventRaised(string value)
    {
        string[] parts = value.Split('|');
        if (parts.Length != 2) return;

        string heroId = parts[0];
        string action = parts[1];

        AllySlot slot = GetSlotByHeroId(heroId);
        if (slot == null) return;

        if (action == "add")
        {
            DeployAlly(slot, heroId);
        }
        else if (action == "remove")
        {
            RemoveAlly(slot);
        }
    }

    private AllySlot GetSlotByHeroId(string heroId)
    {
        foreach (var slot in allySlots)
        {
            if (slot.heroId == heroId)
                return slot;
        }
        return null;
    }

    private void DeployAlly(AllySlot slot, string heroId)
    {
        Hero hero = GetHeroById(heroId);
        if (hero == null)
        {
            Debug.LogWarning($"Không tìm thấy Hero với ID: {heroId}");
            return;
        }

        slot.slotObject.SetActive(true);
        slot.slotObject.transform.position = GetAllySpawnPosition();

        if (slot.slotObject.TryGetComponent(out AllyBaseAI ai))
        {
            ai.Setup(hero);
            AllyManager.Instance.RegisterAlly(ai);
        }

        if (slot.slotObject.TryGetComponent(out AllyStats allyStats))
        {
            if (allyStats.HealthUI == null)
            {
                GameObject ui = Instantiate(CommonReferent.Instance.hpSliderUi, CommonReferent.Instance.canvasHp.transform, false);
                var uiComp = ui.GetComponent<EnemyHealthUI>();
                uiComp.SetTarget(slot.slotObject);
                allyStats.HealthUI = uiComp;
            }
        }

        Debug.Log($"Triệu hồi đồng đội {heroId} thành công.");
    }

    private void RemoveAlly(AllySlot slot)
    {
        if (slot.slotObject.TryGetComponent(out AllyStats allyStats) && allyStats.HealthUI != null)
        {
            allyStats.HealthUI.HideUI();
        }

        slot.slotObject.SetActive(false);
    }

    private Hero GetHeroById(string id)
    {
        foreach (var hero in CommonReferent.Instance.heroManager.allHeroes)
        {
            if (hero.data.id == id)
                return hero;
        }
        return null;
    }

    private Vector3 GetAllySpawnPosition()
    {
        Vector3 playerPos = CommonReferent.Instance.playerPrefab.transform.position;
        return playerPos + new Vector3(Random.Range(-1.5f, 1.5f), 0f, 0f);
    }
}
