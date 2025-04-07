using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HPBarUI : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Slider hpSlider;        // thanh máu chính
    [SerializeField] private Slider delayHpSlider;   // thanh máu trễ (optional)

    [SerializeField] private float delaySpeed = 2f;  // tốc độ delay thu hẹp máu

    private void Start()
    {
        hpSlider.value = 1;
        // Setup ban đầu
        hpSlider.maxValue = playerStats.maxHealth.Value;
        hpSlider.value = playerStats.GetCurrentHealth();

        if (delayHpSlider != null)
        {
            delayHpSlider.maxValue = playerStats.maxHealth.Value;
            delayHpSlider.value = playerStats.GetCurrentHealth();
        }

        playerStats.OnStatsChanged += UpdateMaxHP;
        playerStats.OnHealthChanged += UpdateHP;
    }

    private void UpdateMaxHP()
    {
        hpSlider.maxValue = playerStats.maxHealth.Value;

        if (delayHpSlider != null)
        {
            delayHpSlider.maxValue = playerStats.maxHealth.Value;
        }
    }

    private void UpdateHP()
    {
        hpSlider.value = playerStats.GetCurrentHealth();

        if (delayHpSlider != null)
        {
            StopAllCoroutines();
            StartCoroutine(DelayHPAnimation());
        }
    }

    private IEnumerator DelayHPAnimation()
    {
        yield return new WaitForSeconds(0.2f); // chờ 1 tí rồi mới tụt xuống cho đẹp

        while (delayHpSlider.value > hpSlider.value)
        {
            delayHpSlider.value = Mathf.MoveTowards(delayHpSlider.value, hpSlider.value, delaySpeed * Time.deltaTime);
            yield return null;
        }

        delayHpSlider.value = hpSlider.value;
    }
}