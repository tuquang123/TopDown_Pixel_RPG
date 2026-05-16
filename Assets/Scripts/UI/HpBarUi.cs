using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HPBarUI : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider delayHpSlider;
    [SerializeField] private float delaySpeed = 2f;

    private void Start()
    {
        playerStats = PlayerStats.Instance;

        hpSlider.maxValue = playerStats.maxHealth.Value;
        hpSlider.value    = playerStats.GetCurrentHealth();

        if (delayHpSlider != null)
        {
            delayHpSlider.maxValue = playerStats.maxHealth.Value;
            delayHpSlider.value    = playerStats.GetCurrentHealth();
        }

        playerStats.OnStatsChanged  += UpdateMaxHP;
        playerStats.OnHealthChanged += UpdateHP;
    }

    private void OnDestroy()
    {
        if (playerStats == null) return;
        playerStats.OnStatsChanged  -= UpdateMaxHP;
        playerStats.OnHealthChanged -= UpdateHP;
    }

    private void UpdateMaxHP()
    {
        float newMax = playerStats.maxHealth.Value;
        float current = playerStats.GetCurrentHealth();

        // Update maxValue TRƯỚC rồi mới set value
        hpSlider.maxValue = newMax;
        hpSlider.value    = current;

        if (delayHpSlider != null)
        {
            delayHpSlider.maxValue = newMax;
            delayHpSlider.value    = current;
        }
    }

    private void UpdateHP()
    {
        hpSlider.maxValue = playerStats.maxHealth.Value;
        hpSlider.value    = playerStats.GetCurrentHealth();

        if (delayHpSlider != null)
        {
            StopAllCoroutines();
            StartCoroutine(DelayHPAnimation());
        }
    }

    private IEnumerator DelayHPAnimation()
    {
        yield return new WaitForSeconds(0.2f);

        while (delayHpSlider.value > hpSlider.value)
        {
            delayHpSlider.value = Mathf.MoveTowards(
                delayHpSlider.value, hpSlider.value, delaySpeed * Time.deltaTime);
            yield return null;
        }

        delayHpSlider.value = hpSlider.value;
    }
}