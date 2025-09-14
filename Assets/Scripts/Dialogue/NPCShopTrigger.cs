using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class NPCShopTrigger : MonoBehaviour
{
    [SerializeField] private List<ItemData> shopItems;
    [SerializeField] private Vector3 offset = new Vector3(0, .9f, 0);

    private Button interactButton;
    private Camera mainCam;
    private GameObject targetNPC;

    private void Awake()
    {
        interactButton = CommonReferent.Instance.dialogShopBtn.GetComponent<Button>();
        if (interactButton != null)
            interactButton.gameObject.SetActive(false);

        mainCam = Camera.main;
    }

    private void Start()
    {
        SetTarget(gameObject);
    }

    private void OpenShop()
    {
        var shopUI = UIManager.Instance.GetPopup<ShopUI>();
        if (shopUI != null)
        {
            shopUI.Show();                  
            shopUI.SetupShop(shopItems);    
        }
    }

    public void SetTarget(GameObject npc)
    {
        targetNPC = npc;
        if (interactButton != null)
        {
            interactButton.onClick.RemoveAllListeners();
            interactButton.onClick.AddListener(OpenShop);
        }
    }

    private void LateUpdate()
    {
        if (targetNPC == null || interactButton == null || !interactButton.gameObject.activeSelf)
            return;

        Vector3 worldPos = targetNPC.transform.position + offset;
        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);
        interactButton.transform.position = screenPos;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            ShowUI();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            HideUI();
    }
    
    public void ShowUI()
    {
        if (interactButton == null) return;
        interactButton.gameObject.SetActive(true);
        
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void HideUI() => interactButton?.gameObject.SetActive(false);
}
