using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSlotUI : MonoBehaviour
{
    public Button button;
    public Image icon;
    public Image cooldownOverlay;
    public TextMeshProUGUI nameText;

    private Coroutine cooldownRoutine;

    public void SetSkill(SkillData skill)
    {
        if (skill != null)
        {
            icon.sprite = skill.icon;
            nameText.text = skill.skillName;
            icon.enabled = true;
        }
        else
        {
            nameText.text = "";
            icon.enabled = false;
        }

        cooldownOverlay.fillAmount = 0;
    }

    public void StartCooldown(float cooldown)
    {
        if (cooldownRoutine != null) StopCoroutine(cooldownRoutine);
        cooldownRoutine = StartCoroutine(CooldownCoroutine(cooldown));
    }

    private IEnumerator CooldownCoroutine(float cooldownTime)
    {
        float timeElapsed = 0f;
        cooldownOverlay.fillAmount = 1;

        while (timeElapsed < cooldownTime)
        {
            timeElapsed += Time.deltaTime;
            cooldownOverlay.fillAmount = 1 - (timeElapsed / cooldownTime);
            yield return null;
        }

        cooldownOverlay.fillAmount = 0;
    }
}