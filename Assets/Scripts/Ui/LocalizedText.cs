using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string key;

    private TextMeshProUGUI _textMeshPro;

    private void Awake()
    {
        _textMeshPro = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        UpdateText();
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged += UpdateText;
        }
    }

    private void OnDisable()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged -= UpdateText;
        }
    }

    public void SetKey(string newKey)
    {
        key = newKey;
        UpdateText();
    }

    public void UpdateText()
    {
        if (_textMeshPro == null) _textMeshPro = GetComponent<TextMeshProUGUI>();
        if (_textMeshPro == null) return;

        if (LanguageManager.Instance != null && !string.IsNullOrEmpty(key))
        {
            _textMeshPro.text = LanguageManager.Instance.GetTranslation(key);
        }
    }
}
