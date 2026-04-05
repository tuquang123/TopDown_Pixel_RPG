using System.Collections;
using TMPro;
using UnityEngine;

public class WaveInfoUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private StageManager stageManager;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private TextMeshProUGUI waveBannerText;

    [Header("Banner")]
    [SerializeField] private float bannerDuration = 1.5f;

    private Coroutine bannerRoutine;

    private void Awake()
    {
        waveManager ??= WaveManager.Instance;
        stageManager ??= StageManager.Instance;

        if (waveBannerText != null)
            waveBannerText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (waveManager != null)
        {
            waveManager.OnWaveStarted += HandleWaveStarted;
            waveManager.OnWaveCleared += HandleWaveCleared;
        }

        if (stageManager != null)
            stageManager.OnStageChanged += HandleStageChanged;

        RefreshStaticLabels();
    }

    private void OnDisable()
    {
        if (waveManager != null)
        {
            waveManager.OnWaveStarted -= HandleWaveStarted;
            waveManager.OnWaveCleared -= HandleWaveCleared;
        }

        if (stageManager != null)
            stageManager.OnStageChanged -= HandleStageChanged;
    }

    private void HandleWaveStarted(int wave, bool isBossWave)
    {
        SetWaveText(wave, isBossWave);
        ShowBanner(isBossWave ? $"BOSS WAVE {wave}" : $"WAVE {wave}");
    }

    private void HandleWaveCleared(int wave)
    {
        if (waveText != null)
            waveText.text = $"Wave: {wave} (Cleared)";
    }

    private void HandleStageChanged(int stage)
    {
        if (stageText != null)
            stageText.text = $"Stage: {stage}";
    }

    private void RefreshStaticLabels()
    {
        if (stageText != null)
            stageText.text = $"Stage: {(stageManager != null ? stageManager.CurrentStage : 1)}";

        if (waveText != null)
        {
            int wave = waveManager != null ? waveManager.CurrentWave : 1;
            waveText.text = $"Wave: {Mathf.Max(1, wave)}";
        }
    }

    private void SetWaveText(int wave, bool isBossWave)
    {
        if (waveText == null)
            return;

        waveText.text = isBossWave
            ? $"Wave: {wave} (Boss)"
            : $"Wave: {wave}";
    }

    private void ShowBanner(string message)
    {
        if (waveBannerText == null)
            return;

        if (bannerRoutine != null)
            StopCoroutine(bannerRoutine);

        bannerRoutine = StartCoroutine(BannerRoutine(message));
    }

    private IEnumerator BannerRoutine(string message)
    {
        waveBannerText.text = message;
        waveBannerText.gameObject.SetActive(true);
        yield return new WaitForSeconds(bannerDuration);
        waveBannerText.gameObject.SetActive(false);
        bannerRoutine = null;
    }
}
