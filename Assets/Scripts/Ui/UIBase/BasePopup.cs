using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class BasePopup : MonoBehaviour
{
    private CanvasGroup canvasGroup;
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
        gameObject.SetActive(false);
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);

        fadeTween?.Kill();
        scaleTween?.Kill();

        canvasGroup.alpha = 0f;
        transform.localScale = Vector3.zero;

        fadeTween = canvasGroup.DOFade(1f, 0.2f);
        scaleTween = transform.DOScale(Vector3.one, 0.25f)
            .SetEase(Ease.OutBack);

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        UIManager.Instance?.UpdateBlurState();
    }

    public virtual void Hide()
    {
        fadeTween?.Kill();
        scaleTween?.Kill();

        fadeTween = canvasGroup.DOFade(0f, 0.15f);
        scaleTween = transform.DOScale(Vector3.zero, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                gameObject.SetActive(false);
                
                UIManager.Instance?.UpdateBlurState();
            });
    }
}