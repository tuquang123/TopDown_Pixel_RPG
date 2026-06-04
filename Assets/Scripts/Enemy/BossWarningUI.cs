using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossWarningUI : MonoBehaviour
{
    [Header("Pulse Settings")]
    [SerializeField, Min(0.1f)] private float pulseSpeed     = 2.5f;   // chu kỳ pulse/giây
    [SerializeField]            private float minAlpha       = 0.15f;
    [SerializeField]            private float maxAlpha       = 1f;
    [SerializeField]            private float scaleAmplitude = 0.06f;  // dao động scale (0 = tắt scale)

    [Header("Tint khi pulse lên")]
    [SerializeField] private Color colorHigh = new Color(1f, 0.18f, 0.08f, 1f);
    [SerializeField] private Color colorLow  = new Color(0.6f, 0f,   0f,   1f);

    // ── refs tự tìm ──
    private CanvasGroup      _cg;
    private Image            _bg;
    private TextMeshProUGUI[] _texts;
    private RectTransform    _rect;

    private Vector3 _baseScale;
    private Coroutine _pulseCo;

    private void Awake()
    {
        _cg    = GetComponent<CanvasGroup>();
        if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();
        _cg.blocksRaycasts = false;
        _cg.interactable   = false;

        _bg     = transform.Find("bg")?.GetComponent<Image>();
        _texts  = GetComponentsInChildren<TextMeshProUGUI>(true);
        _rect   = GetComponent<RectTransform>();
        _baseScale = _rect != null ? _rect.localScale : Vector3.one;

        gameObject.SetActive(false);
    }

    // ── WaveManager gọi 2 hàm này ──

    public void SetVisible(bool visible)
    {
        if (visible)
        {
            gameObject.SetActive(true);
            if (_pulseCo != null) StopCoroutine(_pulseCo);
            _pulseCo = StartCoroutine(PulseLoop());
        }
        else
        {
            if (_pulseCo != null) { StopCoroutine(_pulseCo); _pulseCo = null; }
            // reset
            if (_cg   != null) _cg.alpha = 0f;
            if (_rect != null) _rect.localScale = _baseScale;
            gameObject.SetActive(false);
        }
    }

    private IEnumerator PulseLoop()
    {
        float t = 0f;
        while (true)
        {
            // sin 0→1→0
            float s = (Mathf.Sin(t * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;

            // alpha
            if (_cg != null)
                _cg.alpha = Mathf.Lerp(minAlpha, maxAlpha, s);

            // bg color
            if (_bg != null)
                _bg.color = Color.Lerp(colorLow, colorHigh, s);

            // scale pulse nhẹ
            if (_rect != null)
                _rect.localScale = _baseScale * (1f + scaleAmplitude * s);

            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}