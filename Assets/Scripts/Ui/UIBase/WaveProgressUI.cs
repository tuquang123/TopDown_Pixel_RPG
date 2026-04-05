using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class WaveProgressUI : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text statusText;

    [Header("Layout")]
    [SerializeField] private Vector2 rootAnchor = new(0.5f, 1f);
    [SerializeField] private Vector2 rootPosition = new(0f, -30f);

    private WaveManager waveManager;
    private StageManager stageManager;

    private void Awake()
    {
        SetupRoot();
        EnsureLabels();
    }

    private void OnDestroy()
    {
        Unbind();
    }

    public void Bind(WaveManager waveMgr, StageManager stageMgr)
    {
        if (waveMgr == null)
            return;

        Unbind();

        waveManager = waveMgr;
        stageManager = stageMgr;

        waveManager.OnWaveStarted += HandleWaveStarted;
        waveManager.OnWaveCleared += HandleWaveCleared;

        if (stageManager != null)
            stageManager.OnStageChanged += HandleStageChanged;

        RefreshImmediate();
    }

    private void Unbind()
    {
        if (waveManager != null)
        {
            waveManager.OnWaveStarted -= HandleWaveStarted;
            waveManager.OnWaveCleared -= HandleWaveCleared;
        }

        if (stageManager != null)
            stageManager.OnStageChanged -= HandleStageChanged;

        waveManager = null;
        stageManager = null;
    }

    private void SetupRoot()
    {
        RectTransform root = (RectTransform)transform;
        root.anchorMin = rootAnchor;
        root.anchorMax = rootAnchor;
        root.pivot = new Vector2(0.5f, 1f);
        root.anchoredPosition = rootPosition;
        root.sizeDelta = new Vector2(400f, 110f);
    }

    private void EnsureLabels()
    {
        stageText ??= CreateLabel("StageText", new Vector2(0f, 0f), 40, 30);
        waveText ??= CreateLabel("WaveText", new Vector2(0f, -34f), 40, 28);
        statusText ??= CreateLabel("StatusText", new Vector2(0f, -66f), 40, 24);

        stageText.color = new Color(1f, 0.95f, 0.6f, 1f);
        waveText.color = Color.white;
        statusText.color = new Color(0.7f, 0.9f, 1f, 1f);
    }

    private TMP_Text CreateLabel(string labelName, Vector2 anchoredPos, float width, float fontSize)
    {
        GameObject textObj = new GameObject(labelName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(transform, false);

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = new Vector2(width * 10f, 30f);

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = fontSize;
        tmp.raycastTarget = false;
        tmp.text = string.Empty;

        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;

        return tmp;
    }

    private void RefreshImmediate()
    {
        int stage = stageManager != null ? stageManager.CurrentStage : 1;
        int wave = waveManager != null ? waveManager.CurrentWave : 1;

        stageText.text = $"Stage {stage}";
        waveText.text = $"Wave {Mathf.Max(1, wave)}";
        statusText.text = "Preparing...";
    }

    private void HandleWaveStarted(int wave, bool isBossWave)
    {
        waveText.text = $"Wave {wave}";
        statusText.text = isBossWave ? "Boss Wave" : "Enemies Incoming";
    }

    private void HandleWaveCleared(int wave)
    {
        statusText.text = $"Wave {wave} Cleared";
    }

    private void HandleStageChanged(int stage)
    {
        stageText.text = $"Stage {stage}";
        statusText.text = "Stage Up";
    }
}
