using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening; // <-- Thêm dòng này

public class ToastUI : MonoBehaviour, IGameEventListener<string>
{
    [SerializeField] private CanvasGroup toastCanvasGroup; // dùng như template item
    [SerializeField] private float showDuration = 1.15f;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float spawnInterval = 0.08f;
    [SerializeField] private float riseDistance = 70f;
    [SerializeField] private float stackSpacing = 62f;
    [SerializeField] private int maxVisibleToasts = 5;

    private readonly Queue<string> pendingMessages = new();
    private readonly List<RectTransform> activeToasts = new();
    private Coroutine queueRunner;
    private Vector2 templateAnchoredPos;
    private Transform templateParent;

    private void Awake()
    {
        if (toastCanvasGroup == null)
            return;

        RectTransform templateRect = toastCanvasGroup.GetComponent<RectTransform>();
        templateAnchoredPos = templateRect != null ? templateRect.anchoredPosition : Vector2.zero;
        templateParent = toastCanvasGroup.transform.parent;

        // template chỉ để clone, không hiển thị trực tiếp
        toastCanvasGroup.alpha = 0f;
        toastCanvasGroup.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvents.OnShowToast.RegisterListener(this);
    }

    private void OnDisable()
    {
        GameEvents.OnShowToast.UnregisterListener(this);

        if (queueRunner != null)
        {
            StopCoroutine(queueRunner);
            queueRunner = null;
        }

        pendingMessages.Clear();
        activeToasts.Clear();
    }

    public void OnEventRaised(string message)
    {
        if (string.IsNullOrWhiteSpace(message) || toastCanvasGroup == null)
            return;

        pendingMessages.Enqueue(message);

        if (queueRunner == null)
            queueRunner = StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        while (pendingMessages.Count > 0)
        {
            SpawnToastNow(pendingMessages.Dequeue());
            yield return new WaitForSecondsRealtime(spawnInterval);
        }

        queueRunner = null;
    }

    private void SpawnToastNow(string message)
    {
        GameObject toastObj = Instantiate(toastCanvasGroup.gameObject, templateParent);
        toastObj.SetActive(true);

        CanvasGroup cg = toastObj.GetComponent<CanvasGroup>();
        RectTransform rect = toastObj.GetComponent<RectTransform>();
        TextMeshProUGUI text = toastObj.GetComponentInChildren<TextMeshProUGUI>();

        if (cg == null || rect == null || text == null)
        {
            Destroy(toastObj);
            return;
        }

        text.text = message;
        rect.anchoredPosition = templateAnchoredPos;
        cg.alpha = 0f;

        ShiftExistingToastsUp();

        activeToasts.Add(rect);
        TrimOldToastsIfNeeded();

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Join(cg.DOFade(1f, fadeDuration));
        seq.Join(rect.DOAnchorPosY(templateAnchoredPos.y + riseDistance, showDuration + fadeDuration).SetEase(Ease.OutCubic));
        seq.AppendInterval(showDuration);
        seq.Append(cg.DOFade(0f, fadeDuration));
        seq.OnComplete(() =>
        {
            activeToasts.Remove(rect);
            if (toastObj != null)
                Destroy(toastObj);
        });
    }

    private void ShiftExistingToastsUp()
    {
        for (int i = 0; i < activeToasts.Count; i++)
        {
            RectTransform rt = activeToasts[i];
            if (rt == null)
                continue;

            rt.DOAnchorPosY(rt.anchoredPosition.y + stackSpacing, 0.12f)
              .SetEase(Ease.OutQuad)
              .SetUpdate(true);
        }
    }

    private void TrimOldToastsIfNeeded()
    {
        while (activeToasts.Count > maxVisibleToasts)
        {
            RectTransform oldest = activeToasts[0];
            activeToasts.RemoveAt(0);
            if (oldest == null)
                continue;

            CanvasGroup cg = oldest.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.DOFade(0f, 0.08f).SetUpdate(true).OnComplete(() =>
                {
                    if (oldest != null)
                        Destroy(oldest.gameObject);
                });
            }
            else
            {
                Destroy(oldest.gameObject);
            }
        }
    }
}
