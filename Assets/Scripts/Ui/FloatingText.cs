using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingText : MonoBehaviour
{
    public TextMeshProUGUI textMesh;

    public void Setup(string text, Color color)
    {
        textMesh.text = text;
        textMesh.color = color;

        // Tạo độ lệch ngang ngẫu nhiên (âm hoặc dương)
        float randomXOffset = Random.Range(-50f, 50f); // lệch từ -50 đến +50 pixel
        Vector3 targetPosition = transform.position + new Vector3(randomXOffset, 70f, 0f);

        // Di chuyển lên với độ lệch ngẫu nhiên
        transform.DOMove(targetPosition, 1f).SetEase(Ease.OutCubic);

        // Làm mờ rồi huỷ đối tượng
        textMesh.DOFade(0, 1f).SetEase(Ease.Linear).OnComplete(() => Destroy(gameObject));
    }
}
