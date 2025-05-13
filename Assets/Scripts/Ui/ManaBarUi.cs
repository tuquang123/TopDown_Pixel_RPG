using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ManaBarUI : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Slider manaSlider;        // thanh mana chính
    [SerializeField] private Slider delayManaSlider;   // thanh mana trễ (optional)

    [SerializeField] private float delaySpeed = 2f;

    private void Start()
    {
        manaSlider.value = 1;
        manaSlider.maxValue = playerStats.maxMana.Value;
        manaSlider.value = playerStats.currentMana;

        if (delayManaSlider != null)
        {
            delayManaSlider.maxValue = playerStats.maxMana.Value;
            delayManaSlider.value = playerStats.currentMana;
        }

        playerStats.OnStatsChanged += UpdateMaxMana;
        playerStats.OnManaChanged += UpdateMana; // dùng chung event cho đơn giản
    }

    private void UpdateMaxMana()
    {
        manaSlider.maxValue = playerStats.maxMana.Value;

        if (delayManaSlider != null)
        {
            delayManaSlider.maxValue = playerStats.maxMana.Value;
        }
    }

    private void UpdateMana()
    {
        manaSlider.value = playerStats.currentMana;

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
            delayManaSlider.value = Mathf.MoveTowards(delayManaSlider.value, manaSlider.value, delaySpeed * Time.deltaTime);
            yield return null;
        }

        delayManaSlider.value = manaSlider.value;
    }
}