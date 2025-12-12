using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingText : MonoBehaviour
{
    public TextMeshProUGUI textMesh;

    public void Setup(string text, Color color)
    {
        // Replace "gold" → sprite tag
        if (text.Contains("Gold"))
            text = text.Replace("Gold", "<sprite name=\"gold_icon\">");

        if (text.Contains("CRIT"))
            text = text.Replace("CRIT", "<sprite name=\"crit_icon\" color=#FF2B2B> ");


        textMesh.text = text;
        textMesh.color = color;

        float randomXOffset = Random.Range(-50f, 50f);
        Vector3 targetPosition = transform.position + new Vector3(randomXOffset, 100f, 0f);

        transform.DOMove(targetPosition, 1f).SetEase(Ease.OutCubic);
        textMesh.DOFade(0, 1f).SetEase(Ease.Linear).OnComplete(() => Destroy(gameObject));
    }

}
