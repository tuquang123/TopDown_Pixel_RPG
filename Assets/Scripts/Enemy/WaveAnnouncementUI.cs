using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Gắn vào root "WaveAnnouncement".
/// Hiện text "Stage X  Wave Y" trong holdDuration giây rồi tự tắt.
/// WaveManager gọi Show(wave, stage, isBossWave).
/// </summary>
public class WaveAnnouncementUI : MonoBehaviour
{
    [Header("Hold Duration")]
    [SerializeField, Min(0.1f)] private float holdDuration = 2f;

    [Header("Refs (tự tìm nếu để trống)")]
    [SerializeField] private TextMeshProUGUI txtLabel;

    private Coroutine _co;

    private void Awake()
    {
        if (txtLabel == null)
            txtLabel = transform.Find("txtLabel")?.GetComponent<TextMeshProUGUI>();

        gameObject.SetActive(false);
    }

    public void Show(int wave, int stage, bool isBossWave)
    {
        if (txtLabel != null)
            txtLabel.text = $"Stage {stage}  Wave {wave}";

        gameObject.SetActive(true);
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(holdDuration);
        gameObject.SetActive(false);
        _co = null;
    }
}