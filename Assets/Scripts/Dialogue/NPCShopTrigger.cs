using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class NPCShopTrigger : MonoBehaviour
{
    [Header("Shop Settings")]
    [SerializeField] private List<ItemData> shopItems;
    [SerializeField] private Vector3 offset = new Vector3(0, .9f, 0);
    [SerializeField] private float interactRange = 2.5f; // phạm vi mở shop

    private Button interactButton;
    private Camera mainCam;
    private GameObject targetNPC;
    private Transform player;

    private bool isPlayerInRange;

    private void Awake()
    {

        mainCam = Camera.main;
    }

    private void Start()
    {
        interactButton = CommonReferent.Instance.dialogShopBtn.GetComponent<Button>();
        if (interactButton != null)
            interactButton.gameObject.SetActive(false);
        player = CommonReferent.Instance.playerPrefab.transform;
        SetTarget(gameObject);
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool inRange = distance <= interactRange;

        if (inRange && !isPlayerInRange)
        {
            ShowUI();
            isPlayerInRange = true;
        }
        else if (!inRange && isPlayerInRange)
        {
            HideUI();
            isPlayerInRange = false;
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

    private void OpenShop()
    {
        UIManager.Instance.ShowPopupByType(PopupType.Shop);
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

    public void ShowUI()
    {
        if (interactButton == null) return;
        interactButton.gameObject.SetActive(true);

        EventSystem.current?.SetSelectedGameObject(null);
    }

    public void HideUI()
    {
        interactButton?.gameObject.SetActive(false);
    }
}
