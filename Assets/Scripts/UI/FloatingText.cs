using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingText : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    
    public void Setup(string text, Color color, float fontSize = 36f)
    {
        // ── Chuyển Gold sang RewardPopup, không float ──────────────────
        if (text.Contains("Gold"))
        {
            // Parse số lượng từ chuỗi, ví dụ "+ 50 Gold:" hoặc "Gold: +50"
            string digits = System.Text.RegularExpressions.Regex.Match(text, @"\d+").Value;
            if (int.TryParse(digits, out int goldAmount))
                RewardPopupManager.Instance.ShowGold(goldAmount);

            Destroy(gameObject); // không hiện floating text
            return;
        }

        // ── CRIT vẫn float bình thường ─────────────────────────────────
        if (text.Contains("CRIT"))
            text = text.Replace("CRIT", "<sprite name=\"crit_icon\" color=#FF2B2B> ");

        textMesh.text = text;
        textMesh.color = color;
        textMesh.fontSize = fontSize;

        float randomXOffset = Random.Range(-50f, 50f);
        Vector3 targetPosition = transform.position + new Vector3(randomXOffset, 70f, 0f);

        transform.DOMove(targetPosition, 1f).SetEase(Ease.OutCubic);
        textMesh.DOFade(0, 1f)
            .SetEase(Ease.Linear)
            .OnComplete(() => Destroy(gameObject));
    }
}