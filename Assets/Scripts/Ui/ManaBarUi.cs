using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ManaBarUI : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Slider manaSlider;
    [SerializeField] private Slider delayManaSlider;
    [SerializeField] private float delaySpeed = 2f;

    private void Start()
    {
        playerStats = PlayerStats.Instance;

        manaSlider.maxValue = playerStats.maxMana.Value;
        manaSlider.value    = playerStats.currentMana;

        if (delayManaSlider != null)
        {
            delayManaSlider.maxValue = playerStats.maxMana.Value;
            delayManaSlider.value    = playerStats.currentMana;
        }

        playerStats.OnStatsChanged += UpdateMaxMana;
        playerStats.OnManaChanged  += UpdateMana;
    }

    private void OnDestroy()
    {
        if (playerStats == null) return;
        playerStats.OnStatsChanged -= UpdateMaxMana;
        playerStats.OnManaChanged  -= UpdateMana;
    }

    private void UpdateMaxMana()
    {
        float newMax  = playerStats.maxMana.Value;
        float current = playerStats.currentMana;

        // Update maxValue TRƯỚC rồi mới set value
        manaSlider.maxValue = newMax;
        manaSlider.value    = current;

        if (delayManaSlider != null)
        {
            delayManaSlider.maxValue = newMax;
            delayManaSlider.value    = current;
        }
    }

    private void UpdateMana()
    {
        manaSlider.maxValue = playerStats.maxMana.Value;
        manaSlider.value    = playerStats.currentMana;

        if (delayManaSlider != null)
        {
            StopAllCoroutines();
            StartCoroutine(DelayManaAnimation());
        }
    }

    private IEnumerator DelayManaAnimation()
    {
        yield return new WaitForSeconds(0.2f);

        while (delayManaSlider.value > manaSlider.value)
        {
            delayManaSlider.value = Mathf.MoveTowards(
                delayManaSlider.value, manaSlider.value, delaySpeed * Time.deltaTime);
            yield return null;
        }

        delayManaSlider.value = manaSlider.value;
    }
}