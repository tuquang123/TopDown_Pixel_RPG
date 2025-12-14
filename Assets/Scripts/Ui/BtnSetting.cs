using UnityEngine;
using UnityEngine.UI;

public class UISettingController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject settingPanel;

    [Header("Sliders")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        // Ẩn panel khi bắt đầu
        if (settingPanel != null)
            settingPanel.SetActive(false);

        // Init BGM slider
        if (bgmSlider != null && AudioManager.Instance != null)
        {
            bgmSlider.value = AudioManager.Instance.bgmSource.volume;
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }

        // Init SFX slider
        if (sfxSlider != null && AudioManager.Instance != null)
        {
            sfxSlider.value = AudioManager.Instance.sfxSource.volume;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    // Mở / đóng Setting Panel
    public void ToggleSetting()
    {
        if (settingPanel == null) return;

        settingPanel.SetActive(!settingPanel.activeSelf);
    }

    // Đóng Setting Panel (Button Close gọi hàm này)
    public void CloseSetting()
    {
        if (settingPanel != null)
            settingPanel.SetActive(false);
    }

    // Set volume BGM
    private void SetBGMVolume(float value)
    {
        if (AudioManager.Instance != null && AudioManager.Instance.bgmSource != null)
            AudioManager.Instance.bgmSource.volume = value;
    }

    // Set volume SFX
    private void SetSFXVolume(float value)
    {
        if (AudioManager.Instance != null && AudioManager.Instance.sfxSource != null)
            AudioManager.Instance.sfxSource.volume = value;
    }
}