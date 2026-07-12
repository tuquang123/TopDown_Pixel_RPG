using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class WaveAnnouncementUI : MonoBehaviour
{
    [Header("Hold Duration")]
    [SerializeField, Min(0.1f)] private float holdDuration = 2f;

    [Header("Fade Settings")]
    [SerializeField, Min(0f)] private float fadeInDuration  = 0.4f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.4f;

    [Header("Refs (tự tìm nếu để trống)")]
    [SerializeField] private TextMeshProUGUI txtLabel;
    [SerializeField] private BossWarningUI   bossWarningUI; // kéo vào Inspector

    private CanvasGroup _canvasGroup;
    private Coroutine   _co;

    private void Awake()
    {
        if (txtLabel == null)
            txtLabel = transform.Find("txtLabel")?.GetComponent<TextMeshProUGUI>();

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        _canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void Show(int wave, int stage, bool isBossWave)
    {
        // Nếu boss warning đang hiện → bỏ qua
        if (bossWarningUI != null && bossWarningUI.gameObject.activeSelf)
            return;

        if (txtLabel != null)
            txtLabel.text = isBossWave
                ? $"Stage {stage}  — BOSS WAVE —"
                : $"Stage {stage}";

        _canvasGroup.DOKill();
        _canvasGroup.alpha = 0f;
        gameObject.SetActive(true);

        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        _canvasGroup.DOKill();
        yield return _canvasGroup
            .DOFade(1f, fadeInDuration)
            .SetEase(Ease.OutQuad)
            .WaitForCompletion();

        yield return new WaitForSeconds(holdDuration);

        _canvasGroup.DOKill();
        yield return _canvasGroup
            .DOFade(0f, fadeOutDuration)
            .SetEase(Ease.InQuad)
            .WaitForCompletion();

        gameObject.SetActive(false);
        _co = null;
    }

    private void OnDestroy()
    {
        _canvasGroup?.DOKill();
    }
}