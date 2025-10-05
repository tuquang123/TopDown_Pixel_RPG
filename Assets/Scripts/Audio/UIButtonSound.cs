using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    [Header("Tên clip SFX trong AudioManager")]
    public string sfxName = "Click";

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlaySound);
    }

    void PlaySound()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(sfxName))
            AudioManager.Instance.PlaySFX(sfxName);
        else
            Debug.LogWarning($"Không thể phát âm thanh '{sfxName}' – AudioManager chưa sẵn sàng hoặc tên trống.");
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(PlaySound);
    }
}