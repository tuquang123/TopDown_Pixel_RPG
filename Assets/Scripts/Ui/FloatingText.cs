using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingText : MonoBehaviour
{
    public TextMeshProUGUI textMesh;

    public void Setup(string text, Color color, float fontSize = 36f)
    {
        // Replace text → sprite
        if (text.Contains("Gold"))
        {
            text = text.Replace("Gold", "");                  // bỏ chữ Gold
            text = text.Replace(":", "");                     // bỏ :
            text = text.Replace("+ ", "+");                   // gọn dấu +
            text = text.Trim() + " <sprite name=\"gold_icon\">";
        }


        if (text.Contains("CRIT"))
            text = text.Replace("CRIT", "<sprite name=\"crit_icon\" color=#FF2B2B> ");

        textMesh.text = text;
        textMesh.color = color;
        textMesh.fontSize = fontSize;   // ✅ TO / NHỎ TUỲ TRƯỜNG HỢP

        float randomXOffset = Random.Range(-50f, 50f);
        Vector3 targetPosition = transform.position + new Vector3(randomXOffset, 70f, 0f);

        transform.DOMove(targetPosition, 1f).SetEase(Ease.OutCubic);
        textMesh.DOFade(0, 1f)
            .SetEase(Ease.Linear)
            .OnComplete(() => Destroy(gameObject)); // ✅ TỰ BIẾN MẤT
    }
}