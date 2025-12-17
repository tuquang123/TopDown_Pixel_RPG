using UnityEngine;
using UnityEngine.UI;

public class UISettingController : BasePopup
{
    [Header("Sliders")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>() 
                      ?? gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        transform.localScale = Vector3.zero;

        
    }


    private void Start()
    {
        InitSliders();
    }

    private void InitSliders()
    {
        if (AudioManager.Instance == null) return;

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

    // Button OPEN Setting gọi hàm này
    public void OpenSetting()
    {
        Show();
    }

    // Button CLOSE Setting gọi hàm này
    public void CloseSetting()
    {
        Hide();
    }

    private void SetBGMVolume(float value)
    {
        if (AudioManager.Instance?.bgmSource != null)
            AudioManager.Instance.bgmSource.volume = value;
    }

    private void SetSFXVolume(float value)
    {
        if (AudioManager.Instance?.sfxSource != null)
            AudioManager.Instance.sfxSource.volume = value;
    }
}