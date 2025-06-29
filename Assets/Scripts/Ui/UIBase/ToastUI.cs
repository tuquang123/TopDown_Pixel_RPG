using TMPro;
using UnityEngine;
using System.Collections;
using DG.Tweening; // <-- Thêm dòng này

public class ToastUI : MonoBehaviour, IGameEventListener<string>
{
    [SerializeField] private CanvasGroup toastCanvasGroup; // <- Dùng CanvasGroup để fade
    [SerializeField] private TextMeshProUGUI toastText;
    [SerializeField] private float showDuration = 2f;
    [SerializeField] private float fadeDuration = 0.3f;

    private Coroutine currentToast;

    private void Awake()
    {
        // Ẩn ban đầu
        toastCanvasGroup.alpha = 0;
        toastCanvasGroup.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvents.OnShowToast.RegisterListener(this);
    }

    private void OnDisable()
    {
        GameEvents.OnShowToast.UnregisterListener(this);
    }

    public void OnEventRaised(string message)
    {
        if (currentToast != null)
            StopCoroutine(currentToast);

        currentToast = StartCoroutine(ShowToast(message));
    }

    private IEnumerator ShowToast(string message)
    {
        toastText.text = message;
        toastCanvasGroup.gameObject.SetActive(true);
        toastCanvasGroup.alpha = 0;

        // Fade in
        toastCanvasGroup.DOFade(1f, fadeDuration);
        yield return new WaitForSeconds(showDuration);

        // Fade out
        toastCanvasGroup.DOFade(0f, fadeDuration);
        yield return new WaitForSeconds(fadeDuration);

        toastCanvasGroup.gameObject.SetActive(false);
    }
}