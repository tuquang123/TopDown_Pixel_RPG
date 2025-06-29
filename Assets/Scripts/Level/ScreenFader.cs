using UnityEngine;
using DG.Tweening;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadePanel;

    public void FadeIn(float duration, System.Action onComplete = null)
    {
        fadePanel.blocksRaycasts = true;
        fadePanel.DOFade(1, duration).OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }

    public void FadeOut(float duration, System.Action onComplete = null)
    {
        fadePanel.DOFade(0, duration).OnComplete(() =>
        {
            fadePanel.blocksRaycasts = false;
            onComplete?.Invoke();
        });
    }
}