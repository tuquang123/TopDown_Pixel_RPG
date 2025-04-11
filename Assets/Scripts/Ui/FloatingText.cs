using UnityEngine;
using TMPro;
using DG.Tweening; // nhớ import DOTween

public class FloatingText : MonoBehaviour
{
    public TextMeshProUGUI textMesh;

    public void Setup(string text, Color color)
    {
        textMesh.text = text;
        textMesh.color = color;

        transform.DOMoveY(transform.position.y + 100f, 1f).SetEase(Ease.OutCubic); // 100f là pixel trên UI
        textMesh.DOFade(0, 1f).SetEase(Ease.Linear).OnComplete(() => Destroy(gameObject));
    }
}