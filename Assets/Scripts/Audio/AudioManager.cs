using UnityEngine;
using System.Collections.Generic;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public List<AudioClip> bgmClips;
    public List<AudioClip> sfxClips;

    private Dictionary<string, AudioClip> bgmDict = new();
    private Dictionary<string, AudioClip> sfxDict = new();

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return; // Đã bị destroy do trùng instance

        LoadAudioClips();
    }

    void Start()
    {
        Instance.PlayBGM("BGM");
    }

    void LoadAudioClips()
    {
        foreach (var clip in bgmClips)
            bgmDict[clip.name] = clip;

        foreach (var clip in sfxClips)
            sfxDict[clip.name] = clip;
    }

    public void PlayBGM(string clipName, bool loop = true)
    {
        if (bgmDict.TryGetValue(clipName, out var clip))
        {
            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"BGM '{clipName}' không tìm thấy.");
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PlaySFX(string clipName)
    {
        if (sfxDict.TryGetValue(clipName, out var clip))
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"SFX '{clipName}' không tìm thấy.");
        }
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f, float pitch = 1f)
    {
        if (clip == null || sfxSource == null)
            return;

        GameObject oneShot = new GameObject($"SFX_{clip.name}");
        oneShot.transform.SetParent(transform);

        AudioSource source = oneShot.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
        source.volume = sfxSource.volume * volumeScale;
        source.pitch = pitch;
        source.spatialBlend = sfxSource.spatialBlend;
        source.playOnAwake = false;
        source.clip = clip;
        source.Play();

        float lifetime = clip.length / Mathf.Max(0.01f, Mathf.Abs(pitch));
        Destroy(oneShot, lifetime + 0.1f);
    }
}
