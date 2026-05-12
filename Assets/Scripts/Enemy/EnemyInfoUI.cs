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

    private EnemyAI currentEnemy;
    private Coroutine autoHideCoroutine;

    public static EnemyInfoPopupUI Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(EnemyAI enemy)
    {
        if (enemy == null || enemy.IsDead)
        {
            Hide();
            return;
        }

        currentEnemy = enemy;
        gameObject.SetActive(true);
        Refresh();

        if (autoHideCoroutine != null)
            StopCoroutine(autoHideCoroutine);

        autoHideCoroutine = StartCoroutine(AutoHideAfterDelay());
    }

    public void Hide()
    {
        currentEnemy = null;

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
            levelText.text = $"Lv {currentEnemy.EnemyLevel}";

        if (hpSlider != null)
        {
            hpSlider.maxValue = currentEnemy.MaxHealth;
            hpSlider.value    = currentEnemy.CurrentHealth;
        }

        if (hpText != null)
            hpText.text = $"{currentEnemy.CurrentHealth} / {currentEnemy.MaxHealth}";

        if (attackDamageText != null)
            attackDamageText.text = $"ATK: {currentEnemy.AttackDamage}";

        if (moveSpeedText != null)
            moveSpeedText.text = $"SPD: {currentEnemy.MoveSpeed:F1}";
    }

    private IEnumerator AutoHideAfterDelay()
    {
        yield return new WaitForSeconds(autoHideTime);
        Hide();
    }

    private void Update()
    {
        if (currentEnemy == null) return;

        if (currentEnemy.IsDead)
        {
            Hide();
            return;
        }

        if (hpSlider != null)
            hpSlider.value = currentEnemy.CurrentHealth;

        if (hpText != null)
            hpText.text = $"{currentEnemy.CurrentHealth} / {currentEnemy.MaxHealth}";
    }
}