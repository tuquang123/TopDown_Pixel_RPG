using UnityEngine;
using UnityEngine.UI;

public class UISettingController : BasePopup
{
    [Header("Sliders")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private const string BGM_KEY = "BGM_VOLUME";
    private const string SFX_KEY = "SFX_VOLUME";

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
        LoadVolume();
        InitSliders();
    }

    private void LoadVolume()
    {
        if (AudioManager.Instance == null) return;

        float bgm = PlayerPrefs.GetFloat(BGM_KEY, 1f);
        float sfx = PlayerPrefs.GetFloat(SFX_KEY, 1f);

        AudioManager.Instance.bgmSource.volume = bgm;
        AudioManager.Instance.sfxSource.volume = sfx;
    }

    private void InitSliders()
    {
        if (AudioManager.Instance == null) return;

        if (bgmSlider != null)
        {
            bgmSlider.SetValueWithoutNotify(AudioManager.Instance.bgmSource.volume);
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(AudioManager.Instance.sfxSource.volume);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    public void OpenSetting()
    {
        Show();
    }

    public void CloseSetting()
    {
        Hide();
    }

    private void SetBGMVolume(float value)
    {
        if (AudioManager.Instance?.bgmSource == null) return;

        AudioManager.Instance.bgmSource.volume = value;
        PlayerPrefs.SetFloat(BGM_KEY, value);
        PlayerPrefs.Save();
    }

    private void SetSFXVolume(float value)
    {
        if (AudioManager.Instance?.sfxSource == null) return;

        AudioManager.Instance.sfxSource.volume = value;
        PlayerPrefs.SetFloat(SFX_KEY, value);
        PlayerPrefs.Save();
    }
}
