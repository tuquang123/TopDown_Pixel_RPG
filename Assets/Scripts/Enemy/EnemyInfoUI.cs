using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class EnemyInfoPopupUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Auto Hide")]
    [SerializeField] private float autoHideTime = 3f; // ⭐ 3 giây

    private EnemyAI currentEnemy;
    private Coroutine autoHideCoroutine; // ⭐

    public static EnemyInfoPopupUI Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    // ====== PUBLIC API ======
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

        // ⭐ reset đếm 3 giây mỗi lần Show
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

    private IEnumerator AutoHideAfterDelay() // ⭐
    {
        yield return new WaitForSeconds(autoHideTime);
        Hide();
    }

    public void Refresh()
    {
        if (currentEnemy == null) return;

        nameText.text = currentEnemy.EnemyName;
        levelText.text = $"Lv {currentEnemy.EnemyLevel}";

        hpSlider.maxValue = currentEnemy.MaxHealth;
        hpSlider.value = currentEnemy.CurrentHealth;

        hpText.text = $"{currentEnemy.CurrentHealth} / {currentEnemy.MaxHealth}";
    }

    private void Update()
    {
        // Enemy chết → đóng popup ngay
        if (currentEnemy != null && currentEnemy.IsDead)
        {
            Hide();
        }
    }
}
