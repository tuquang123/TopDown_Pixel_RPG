using UnityEngine;

public class TeamAssistHandler : MonoBehaviour, IGameEventListener<string>
{
    public GameObject slot1;
    public GameObject slot2;
    
    [SerializeField] private HeroManager heroManager;
    
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

        if (action == "add")
        {
            if (heroId == "h_jack")
            {
                DeployArcher();
            }
            else if (heroId == "h_virus")
            {
                DeployWarious();
            }

            ResetSlotsToPlayer();
        }
        else if (action == "remove")
        {
            GameObject slot = null;

            if (heroId == "h_jack") slot = slot1;
            else if (heroId == "h_virus") slot = slot2;

            if (slot != null)
            {
                var stats = slot.GetComponent<AllyStats>();
                if (stats != null && stats.HealthUI != null)
                {
                    stats.HealthUI.HideUI(); // Ẩn thanh máu
                }

                slot.SetActive(false); // Tắt ally khỏi game
            }

            Debug.Log($"Gỡ đồng đội {heroId} khỏi slot.");
        }

    }
    private void DeployArcher()
    {
        Hero hero = GetHeroById("h_jack");
        if (hero == null)
        {
            Debug.LogWarning("Không tìm thấy Hero với ID: h_jack");
            return;
        }

        slot1.SetActive(true);

        var ai = slot1.GetComponent<ArcherAI>();
        if (ai != null)
        {
            ai.Setup(hero);
            
            AllyStats allyStats = ai.GetComponent<AllyStats>();
            if (allyStats != null && allyStats.HealthUI == null)
            {
                GameObject ui = Instantiate(RefVFX.Instance.hpSliderUi , RefVFX.Instance.canvasHp.transform, false);
                var uiComp = ui.GetComponent<EnemyHealthUI>();
                uiComp.SetTarget(slot1); 
                allyStats.HealthUI = uiComp;
            }
        }

        Debug.Log("Đã kích hoạt Archer và gán chỉ số.");
    }
    
    private Hero GetHeroById(string id)
    {
        foreach (var hero in heroManager.allHeroes)
        {
            if (hero.data.id == id)
                return hero;
        }
        return null;
    }


    private void DeployWarious()
    {
        Hero hero = GetHeroById("h_virus");
        if (hero == null)
        {
            Debug.LogWarning("Không tìm thấy Hero với ID: h_virus");
            return;
        }

        slot2.SetActive(true);

        WarriorAi ai = slot2.GetComponent<WarriorAi>();
        if (ai != null)
        {
            ai.Setup(hero);
            
            AllyStats allyStats = ai.GetComponent<AllyStats>();
            if (allyStats != null && allyStats.HealthUI == null)
            {
                GameObject ui = Instantiate(RefVFX.Instance.hpSliderUi , RefVFX.Instance.canvasHp.transform, false);
                var uiComp = ui.GetComponent<EnemyHealthUI>();
                uiComp.SetTarget(slot2); 
                allyStats.HealthUI = uiComp;
            }
        }

        Debug.Log("Đã kích hoạt Warious và gán chỉ số.");
    }

    public void RemoveHero(string heroId)
    {
        if (heroId == "h_jack")
        {
            slot1.SetActive(false);
        }
        else if (heroId == "h_virus")
        {
            slot2.SetActive(false);
        }
    
        Debug.Log($"Đã gỡ hero {heroId} khỏi trận và tắt slot.");
    }


    private void ResetSlotsToPlayer()
    {
        Vector3 playerPos = gameObject.transform.position;

        // Đặt lại vị trí slot1 và slot2 gần người chơi
        slot1.transform.position = playerPos + new Vector3(-1f, 0f, 0f); // lệch trái
        slot2.transform.position = playerPos + new Vector3(1f, 0f, 0f); // lệch phải

        Debug.Log("Đặt lại đồng đội về gần vị trí người chơi.");
    }
}
