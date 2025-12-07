using UnityEngine;
using UnityEngine.UI;

public class UISettingController : MonoBehaviour
{
    [Header("Panel")]
    public GameObject settingPanel;

    [Header("Sliders")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    void Start()
    {
        // Hiển thị panel theo mặc định (có thể false nếu muốn ẩn)
        if (settingPanel != null)
            settingPanel.SetActive(false);

        // Khởi tạo giá trị slider theo AudioManager
        if (bgmSlider != null)
        {
            bgmSlider.value = AudioManager.Instance.bgmSource.volume;
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = AudioManager.Instance.sfxSource.volume;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    // Toggle panel Setting
    public void ToggleSetting()
    {
        if (settingPanel != null)
            settingPanel.SetActive(!settingPanel.activeSelf);
    }

    // Set volume BGM
    private void SetBGMVolume(float value)
    {
        if (AudioManager.Instance.bgmSource != null)
            AudioManager.Instance.bgmSource.volume = value;
    }

    // Set volume SFX
    private void SetSFXVolume(float value)
    {
        if (AudioManager.Instance.sfxSource != null)
            AudioManager.Instance.sfxSource.volume = value;
    }
}
