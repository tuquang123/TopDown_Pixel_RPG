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

    protected virtual bool ShouldDestroyOnHide() => true;

    public virtual void Show()
    {
        fadeTween?.Kill();
        scaleTween?.Kill();

        gameObject.SetActive(true);

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

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        fadeTween = canvasGroup
            .DOFade(0f, 0.15f)
            .SetUpdate(true);

        scaleTween = transform
            .DOScale(Vector3.zero, 0.2f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                UIManager.Instance?.UpdateBlurState();

                if (ShouldDestroyOnHide())
                    Destroy(gameObject);
                else
                    gameObject.SetActive(false);
            });
    }
}