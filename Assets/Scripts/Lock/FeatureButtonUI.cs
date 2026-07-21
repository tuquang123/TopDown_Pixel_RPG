using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FeatureButtonUI : MonoBehaviour
{
    [SerializeField] private FeatureType featureType;
    [SerializeField] private FeatureUnlockData unlockData;
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private Button button;

    private PlayerLevel _playerLevel;
    private UIButtonPopupLink _popupLink;

    void Awake()
    {
        _playerLevel = FindObjectOfType<PlayerLevel>();
        _popupLink = GetComponent<UIButtonPopupLink>();

        // Set isLocked ngay trong Awake trước khi bất kỳ thứ gì chạy
        SyncLockFlag();
    }

    void Start()
    {
        RefreshLockState();
        button.onClick.AddListener(OnClick);
    }

    // Chỉ sync flag, không animation — dùng trong Awake
    private void SyncLockFlag()
    {
        if (_popupLink == null) return;
        var entry = unlockData?.Get(featureType);
        if (entry == null) { _popupLink.isLocked = false; return; }

        bool unlocked = GetCurrentLevel() >= entry.requiredLevel;
        _popupLink.isLocked = !unlocked;
    }

    public void RefreshLockState()
    {
        if (_popupLink == null)
            _popupLink = GetComponent<UIButtonPopupLink>();

        if (_playerLevel == null)
            _playerLevel = FindObjectOfType<PlayerLevel>();

        var entry = unlockData.Get(featureType);
        if (entry == null || lockOverlay == null) return;

        bool unlocked = GetCurrentLevel() >= entry.requiredLevel;

        if (_popupLink != null)
            _popupLink.isLocked = !unlocked;

        if (unlocked && lockOverlay.activeSelf)
        {
            lockOverlay.transform.DOKill();
            lockOverlay.transform.localScale = Vector3.one;
            lockOverlay.transform.DOScale(0f, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() => lockOverlay.SetActive(false));

            transform.DOKill();
            transform.localScale = Vector3.one;
            transform.DOPunchScale(Vector3.one * 0.2f, 0.4f, 8, 0.5f);
        }
        else if (!unlocked && !lockOverlay.activeSelf)
        {
            lockOverlay.SetActive(true);
            lockOverlay.transform.DOKill();
            lockOverlay.transform.localScale = Vector3.zero;
            lockOverlay.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }
        else
        {
            lockOverlay.SetActive(!unlocked);
        }
    }

    private void OnClick()
    {
        var entry = unlockData.Get(featureType);
        if (entry == null) return;

        bool unlocked = GetCurrentLevel() >= entry.requiredLevel;

        if (!unlocked)
        {
            transform.DOKill();
            transform.localScale = Vector3.one;
            transform.DOShakePosition(0.4f, strength: new Vector3(8f, 0f, 0f), vibrato: 20, randomness: 0);

            string message = GetLockedMessage(entry);

            if (ToastUI.Instance != null)
                ToastUI.Instance.ShowToast(message);
            else
                GameEvents.OnShowToast.Raise(message);

            return;
        }
    }
    
    private int GetCurrentLevel()
    {
        if (_playerLevel == null) return 1;
        return _playerLevel.levelSystem.level;
    }

    private string GetRequiredLevelMessage(int requiredLevel)
    {
        string template = LanguageManager.Instance != null
            ? LanguageManager.Instance.GetTranslationOrFallback("feature_requires_level", "Requires level {0}")
            : "Requires level {0}";

        return string.Format(template, requiredLevel);
    }

    private string GetLockedMessage(FeatureUnlockEntry entry)
    {
        if (entry == null)
            return string.Empty;

        string message = entry.lockedMessage;
        if (string.IsNullOrWhiteSpace(message) || IsDefaultRequiredLevelMessage(message))
            return GetRequiredLevelMessage(entry.requiredLevel);

        return LanguageManager.Instance != null
            ? LanguageManager.Instance.TranslateText(message)
            : message;
    }

    private bool IsDefaultRequiredLevelMessage(string message)
    {
        return message.Trim().StartsWith("Requires level", System.StringComparison.OrdinalIgnoreCase);
    }

    private void OnDestroy()
    {
        transform.DOKill();
        if (lockOverlay != null)
            lockOverlay.transform.DOKill();
    }
}
