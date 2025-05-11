using UnityEngine;

public class TeamAssistHandler : MonoBehaviour, IGameEventListener<string>
{
    public GameObject slot1;
    public GameObject slot2;

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
            if (heroId == "h_jack")
            {
                slot1.SetActive(false);
            }
            else if (heroId == "h_virus")
            {
                slot2.SetActive(false);
            }

            Debug.Log($"Gỡ đồng đội {heroId} khỏi slot.");
        }
    }
    private void DeployArcher()
    {
        // Instantiate archer prefab, setup, v.v.
        slot1.SetActive(true);
    }

    private void DeployWarious()
    {
        // Instantiate mage prefab, setup, v.v.
        slot2.SetActive(true);
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
