using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveProgressUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text waveText;

    [Header("Points")]
    [SerializeField] private Image starPoint;   // wave 1
    [SerializeField] private Image point2;      // wave 2
    [SerializeField] private Image point3; 
    [SerializeField] private Image point4;    

    [SerializeField] private Image bossPoint;   // boss

    [Header("Progress Fill")]
    [SerializeField] private Slider progressFill; // thanh kéo dài
    private Image[] points;

    private WaveManager waveManager;
    private int currentWave;
    private int bossFreq = 4;

    private Coroutine pulseRoutine;

    // màu
    private Color done    = new Color(1f, 0.8f, 0.3f, 1f);
    private Color current = Color.white;
    private Color empty   = new Color(1f, 1f, 1f, 0.2f);
    private Color bossOn  = new Color(1f, 0.3f, 0.3f, 1f);

    // smooth fill
    private float targetFill;
    private float currentFill;

    void Awake()
    {
        points = new Image[] { starPoint, point2, point3, point4  };
    }

    public void Bind(WaveManager wm)
    {
        waveManager = wm;
        bossFreq = Mathf.Max(2, wm.BossWaveFrequency);

        wm.OnWaveStarted += OnWaveStart;
        wm.OnWaveCleared += OnWaveClear;

        Refresh();
    }
        
    private void Update()
    {
        // smooth progress
        if (progressFill != null)
        {
            currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * 6f);
            progressFill.value = currentFill;
        }
    }

    private void OnWaveStart(int wave , int stage, bool isBoss)
    {
        currentWave = wave;

        if (waveText != null)
        {
            waveText.text = $"Stage {stage}   Wave {wave}";
        }

        Refresh();

        if (isBoss) StartPulse();
        else StopPulse();
    }
    private void OnWaveClear(int wave)
    {
        Refresh();
        StopPulse();
    }

    private void Refresh()
    {
        int pos = currentWave <= 0 ? 0 : ((currentWave - 1) % bossFreq) + 1;

        // reset màu
        foreach (var p in points)
            if (p != null) p.color = empty;

        if (bossPoint != null)
            bossPoint.color = empty;

        // fill point thường
        for (int i = 0; i < points.Length; i++)
        {
            int slot = i + 1;

            if (slot < pos) points[i].color = done;
            else if (slot == pos) points[i].color = current;
        }

        // boss
        if (pos == bossFreq && bossPoint != null)
            bossPoint.color = bossOn;

        // progress
        int totalSteps = bossFreq;
        float progress = 0f;

        if (currentWave > 0)
            progress = (float)(pos - 1) / (totalSteps - 1);

        targetFill = progress;
    }

    // ───────── boss pulse ─────────
    private void StartPulse()
    {
        StopPulse();
        if (bossPoint != null)
            pulseRoutine = StartCoroutine(Pulse());
    }

    private void StopPulse()
    {
        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
            pulseRoutine = null;
        }
    }

    private IEnumerator Pulse()
    {
        while (true)
        {
            float t = (Mathf.Sin(Time.time * 4f) + 1f) / 2f;
            bossPoint.color = Color.Lerp(bossOn, Color.white, t);
            yield return null;
        }
    }
   
}