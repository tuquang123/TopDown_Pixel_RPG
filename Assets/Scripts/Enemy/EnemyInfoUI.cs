using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class EnemyInfoPopupUI : MonoBehaviour
{
    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("HP")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI attackDamageText;
    [SerializeField] private TextMeshProUGUI moveSpeedText;

    [Header("Auto Hide")]
    [SerializeField] private float autoHideTime = 4f;
    [SerializeField] private float hideOnDeathDelay = 1f;

    private EnemyAI currentEnemy;
    private Coroutine autoHideCoroutine;
    private bool isDyingHide = false;

    public static EnemyInfoPopupUI Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (LanguageManager.Instance != null)
            LanguageManager.Instance.OnLanguageChanged += Refresh;

        PrepareTextLayout();
    }

    private void OnDisable()
    {
        if (LanguageManager.Instance != null)
            LanguageManager.Instance.OnLanguageChanged -= Refresh;
    }

    public void Show(EnemyAI enemy)
    {
        if (enemy == null || enemy.IsDead)
        {
            Hide();
            return;
        }

        currentEnemy = enemy;
        isDyingHide = false;
        gameObject.SetActive(true);
        Refresh();

        if (autoHideCoroutine != null)
            StopCoroutine(autoHideCoroutine);

        autoHideCoroutine = StartCoroutine(AutoHideAfterDelay(autoHideTime));
    }

    public void Hide()
    {
        currentEnemy = null;
        isDyingHide = false;

        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
            autoHideCoroutine = null;
        }

        gameObject.SetActive(false);
    }

    public void Refresh()
    {
        if (currentEnemy == null) return;

        if (nameText != null)
            nameText.text = currentEnemy.EnemyName;

        if (levelText != null)
            levelText.text = $"{GetLevelLabel()} {currentEnemy.EnemyLevel}";

        float displayHp = Mathf.Max(0, currentEnemy.CurrentHealth);

        if (hpSlider != null)
        {
            hpSlider.maxValue = currentEnemy.MaxHealth;
            hpSlider.value    = displayHp;
        }

        if (hpText != null)
            hpText.text = $"{displayHp} / {currentEnemy.MaxHealth}";

        if (attackDamageText != null)
            attackDamageText.text = $"{GetAttackLabel()}: {currentEnemy.AttackDamage}";

        if (moveSpeedText != null)
            moveSpeedText.text = $"{GetSpeedLabel()}: {currentEnemy.MoveSpeed:F1}";
    }

    private void Update()
    {
        if (currentEnemy == null) return;

        if (currentEnemy.IsDead)
        {
            if (!isDyingHide)
            {
                isDyingHide = true;

                if (autoHideCoroutine != null)
                    StopCoroutine(autoHideCoroutine);

                autoHideCoroutine = StartCoroutine(AutoHideAfterDelay(hideOnDeathDelay));
            }
            return;
        }

        float displayHp = Mathf.Max(0, currentEnemy.CurrentHealth);

        if (hpSlider != null)
            hpSlider.value = displayHp;

        if (hpText != null)
            hpText.text = $"{displayHp} / {currentEnemy.MaxHealth}";
    }

    private IEnumerator AutoHideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Hide();
    }

    private void PrepareTextLayout()
    {
        PrepareText(nameText, 18f, 43.2f);
        PrepareText(levelText, 12f, 30.25f);
        PrepareText(hpText, 12f, 29.85f);
        PrepareText(attackDamageText, 12f, 36f);
        PrepareText(moveSpeedText, 12f, 36f);
    }

    private void PrepareText(TextMeshProUGUI text, float minSize, float maxSize)
    {
        if (text == null) return;

        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.enableAutoSizing = true;
        text.fontSizeMin = minSize;
        text.fontSizeMax = maxSize;
    }

    private string GetLevelLabel()
    {
        return LanguageManager.Instance != null && LanguageManager.Instance.CurrentLanguage == "vi"
            ? "C\u1ea5p"
            : "Lv";
    }

    private string GetAttackLabel()
    {
        return LanguageManager.Instance != null && LanguageManager.Instance.CurrentLanguage == "vi"
            ? "TC"
            : "ATK";
    }

    private string GetSpeedLabel()
    {
        return LanguageManager.Instance != null && LanguageManager.Instance.CurrentLanguage == "vi"
            ? "T\u0110"
            : "SPD";
    }
}
