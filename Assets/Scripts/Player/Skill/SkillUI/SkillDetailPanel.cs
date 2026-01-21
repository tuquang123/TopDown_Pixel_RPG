using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SkillDetailPanel : MonoBehaviour
{
    // 🔥 EVENT BÁO UI REFRESH
    public static event Action OnSkillChanged;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI skillPointText;

    [Header("Buttons")]
    [SerializeField] private Button learnButton;
    [SerializeField] private Button assignButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button closeButtonFull;

    [Header("Lock UI")]
    [SerializeField] private GameObject lockRoot;
    [Header("Dim UI")]
    [SerializeField] private Image dimImage;

    [Header("Assign")]
    [SerializeField] private SkillAssignPanel assignPanel;

    private SkillData currentSkill;
    private SkillSystem skillSystem;
    [Header("Animation")]
    [SerializeField] private float animDuration = 0.25f;

    private CanvasGroup canvasGroup;
    private Coroutine animCoroutine;

    // =========================
    // SETUP
    // =========================
    public void Setup(SkillData skillData, SkillSystem system)
    {
        currentSkill = skillData;
        skillSystem = system;

        RefreshUI();        // chỉ update nội dung
        HookButtons();
        PlayOpenAnimation(); // chỉ chạy khi mở panel lần đầu
    }
    private void RefreshUI()
    {
        ResetLockState();

        int skillPoint = PlayerStats.Instance.skillPoints;
        int currentLevel = skillSystem.GetSkillLevel(currentSkill.skillID);
        bool hasSkillPoint = skillPoint > 0;
        bool isUnlocked = currentLevel > 0;
        bool isMaxLevel = currentLevel >= currentSkill.maxLevel;
        bool isActive = currentSkill.skillType == SkillType.Active;

        skillPointText.text = $"Skill Point : {skillPoint}";
        nameText.text = currentSkill.skillName;
        iconImage.sprite = currentSkill.icon;

        levelText.text = $"Level: {currentLevel}/{currentSkill.maxLevel}";
        descriptionText.text = BuildDescription(currentLevel);

        if (!isUnlocked)
        {
            dimImage.enabled = true;
            lockRoot.SetActive(true);

            if (hasSkillPoint)
            {
                learnButton.gameObject.SetActive(true);
                learnButton.interactable = true;
                learnButton.GetComponentInChildren<TextMeshProUGUI>().text = "Learn";
            }
        }
        else
        {
            dimImage.enabled = false;
            lockRoot.SetActive(false);

            if (!isMaxLevel && hasSkillPoint)
            {
                learnButton.gameObject.SetActive(true);
                learnButton.interactable = true;
                learnButton.GetComponentInChildren<TextMeshProUGUI>().text = "Upgrade";
            }

            if (isActive)
                assignButton.gameObject.SetActive(true);
        }
    }


    private void ResetLockState()
    {
        lockRoot.SetActive(false);

        learnButton.gameObject.SetActive(false);
        learnButton.interactable = true; // 🔴 QUAN TRỌNG

        assignButton.gameObject.SetActive(false);

        if (dimImage != null)
            dimImage.enabled = false;
    }


    // =========================
    // DESCRIPTION
    // =========================
    private string BuildDescription(int currentLevel)
    {
        string result = "";

        if (currentLevel > 0)
        {
            SkillLevelStat currentStat = currentSkill.GetLevelStat(currentLevel);
            if (currentStat != null)
            {
                result += $"<b>Current (Level {currentLevel}):</b>\n";
                result += FormatDesc(currentStat) + "\n\n";
            }
        }

        int nextLevel = currentLevel + 1;
        if (nextLevel <= currentSkill.maxLevel)
        {
            SkillLevelStat nextStat = currentSkill.GetLevelStat(nextLevel);
            if (nextStat != null)
            {
                result += $"<b>Next (Level {nextLevel}):</b>\n";
                result += FormatDesc(nextStat);
            }
        }

        return result;
    }

    private string FormatDesc(SkillLevelStat stat)
    {
        return currentSkill.descriptionTemplate
            .Replace("{value}", stat.value.ToString())
            .Replace("{mana}", stat.manaCost.ToString())
            .Replace("{cooldown}", stat.cooldown.ToString("0.#"))
            .Replace("{duration}", stat.duration.ToString("0.#"));
    }

    // =========================
    // BUTTONS
    // =========================
    private void HookButtons()
    {
        learnButton.onClick.RemoveAllListeners();
        assignButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();
        closeButtonFull.onClick.RemoveAllListeners();

        learnButton.onClick.AddListener(OnClickLearn);
        assignButton.onClick.AddListener(OnClickAssign);
        closeButton.onClick.AddListener(Hide);
        closeButtonFull.onClick.AddListener(Hide);
    }

    private void OnClickLearn()
    {
        learnButton.interactable = false;

        if (!skillSystem.UnlockSkill(currentSkill.skillID))
        {
            learnButton.interactable = true;
            return;
        }

        skillSystem.DecrementSkillPoint();

        if (currentSkill.skillType == SkillType.Passive)
            skillSystem.UseSkill(currentSkill.skillID);

        OnSkillChanged?.Invoke();

        RefreshUI();   // ✅ chỉ update UI, KHÔNG animation, KHÔNG reset panel
    }





    private void OnClickAssign()
    {
        assignPanel.Show(currentSkill, skillSystem);
    }

    // =========================
    // LOCK STATE
    // =========================
  

    private void ShowLocked()
    {
        lockRoot.SetActive(true);
    }

    // =========================
    // VISIBILITY
    // =========================
    public void Hide()
    {
        assignPanel.Hide();

        if (!gameObject.activeSelf) return;

        if (animCoroutine != null)
            StopCoroutine(animCoroutine);

        animCoroutine = StartCoroutine(CloseAnim());
    }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }
    private void PlayOpenAnimation()
    {
        if (animCoroutine != null)
            StopCoroutine(animCoroutine);

        gameObject.SetActive(true);
        animCoroutine = StartCoroutine(OpenAnim_Pop());
    }

    private IEnumerator OpenAnim_Pop()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        transform.localScale = Vector3.zero;

        float t = 0f;
        while (t < animDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = t / animDuration;

            float scale = EaseOutBack(p);
            transform.localScale = Vector3.one * scale;
            canvasGroup.alpha = p;

            yield return null;
        }

        transform.localScale = Vector3.one;
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private float EaseOutBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
    }
    private IEnumerator CloseAnim()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float t = 0f;
        Vector3 startScale = transform.localScale;

        while (t < animDuration * 0.7f)
        {
            t += Time.unscaledDeltaTime;
            float p = t / (animDuration * 0.7f);

            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, p);
            canvasGroup.alpha = 1 - p;
            yield return null;
        }

        canvasGroup.alpha = 0;
        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }

    
}
