using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class BasePopup : MonoBehaviour
{
    protected CanvasGroup canvasGroup;
    private Tween fadeTween;
    private Tween scaleTween;

    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        transform.localScale = Vector3.zero;
    }

    public virtual void Show()
    {
        Time.timeScale = 0f; // pause game

        fadeTween?.Kill();
        scaleTween?.Kill();

        fadeTween = canvasGroup
            .DOFade(1f, 0.2f)
            .SetUpdate(true);

        scaleTween = transform
            .DOScale(Vector3.one, 0.25f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        UIManager.Instance?.UpdateBlurState();
    }

    public virtual void Hide()
    {
        fadeTween?.Kill();
        scaleTween?.Kill();

        fadeTween = canvasGroup
            .DOFade(0f, 0.15f)
            .SetUpdate(true);

        scaleTween = transform
            .DOScale(Vector3.zero, 0.2f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                Time.timeScale = 1f; // resume game
                UIManager.Instance?.UpdateBlurState();
                Destroy(gameObject); // 🔥 destroy popup
            });
    }
}
