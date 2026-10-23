using UnityEngine;
using System.Collections.Generic;

public enum IdleGameSfx
{
    Attack,
    PlayerDeath,
    EnemyDefeated,
    EnemyAttack,
    BossDefeated,
    BossWarning,
    PlayerLevelUp,
    EquipmentUpgrade,
    EquipmentEquip,
    GachaRoll,
    QuestReward,
    ItemBuy,
    ItemSell,
    StatUpgrade
}

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public List<AudioClip> bgmClips;
    public List<AudioClip> sfxClips;

    [Header("Idle Game SFX")]
    [Tooltip("Kéo file âm thanh từ Assets/Prefab/Audio vào đúng ô tương ứng.")]
    [SerializeField] private AudioClip attackSfx;
    [SerializeField] private AudioClip playerDeathSfx;
    [SerializeField] private AudioClip enemyDefeatedSfx;
    [SerializeField] private AudioClip enemyAttackSfx;
    [SerializeField] private AudioClip bossDefeatedSfx;
    [SerializeField] private AudioClip bossWarningSfx;
    [SerializeField] private AudioClip playerLevelUpSfx;
    [SerializeField] private AudioClip equipmentUpgradeSfx;
    [SerializeField] private AudioClip equipmentEquipSfx;
    [SerializeField] private AudioClip gachaRollSfx;
    [SerializeField] private AudioClip questRewardSfx;
    [SerializeField] private AudioClip itemBuySfx;
    [SerializeField] private AudioClip itemSellSfx;
    [SerializeField] private AudioClip statUpgradeSfx;

    [Header("Boss Battle Music")]
    [Tooltip("Nhạc lặp khi boss xuất hiện và tự tắt khi boss chết.")]
    [SerializeField] private AudioClip bossBattleMusic;
    [Range(0f, 1f)] [SerializeField] private float bossMusicVolume = 0.45f;
    private AudioSource bossMusicSource;
    private bool bgmPausedForBossMusic;

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

    public void PlayIdleGameSfx(IdleGameSfx sound, float volumeScale = 1f)
    {
        AudioClip clip = sound switch
        {
            IdleGameSfx.Attack => attackSfx,
            IdleGameSfx.PlayerDeath => playerDeathSfx,
            IdleGameSfx.EnemyDefeated => enemyDefeatedSfx,
            IdleGameSfx.EnemyAttack => enemyAttackSfx,
            IdleGameSfx.BossDefeated => bossDefeatedSfx,
            IdleGameSfx.BossWarning => bossWarningSfx,
            IdleGameSfx.PlayerLevelUp => playerLevelUpSfx,
            IdleGameSfx.EquipmentUpgrade => equipmentUpgradeSfx,
            IdleGameSfx.EquipmentEquip => equipmentEquipSfx,
            IdleGameSfx.GachaRoll => gachaRollSfx,
            IdleGameSfx.QuestReward => questRewardSfx,
            IdleGameSfx.ItemBuy => itemBuySfx,
            IdleGameSfx.ItemSell => itemSellSfx,
            IdleGameSfx.StatUpgrade => statUpgradeSfx,
            _ => null
        };

        if (clip == null)
        {
            Debug.LogWarning($"Idle SFX '{sound}' chưa được gán trong AudioManager.");
            return;
        }

        PlaySFX(clip, volumeScale);
    }

    public void PlayBossBattleMusic()
    {
        if (bossBattleMusic == null)
        {
            Debug.LogWarning("Boss Battle Music chưa được gán trong AudioManager.");
            return;
        }

        if (bossMusicSource == null)
        {
            GameObject musicObject = new GameObject("Boss Battle Music");
            musicObject.transform.SetParent(transform, false);
            bossMusicSource = musicObject.AddComponent<AudioSource>();
            bossMusicSource.outputAudioMixerGroup = bgmSource != null ? bgmSource.outputAudioMixerGroup : null;
            bossMusicSource.spatialBlend = 0f;
            bossMusicSource.playOnAwake = false;
        }

        if (bossMusicSource.isPlaying && bossMusicSource.clip == bossBattleMusic)
            return;

        bossMusicSource.clip = bossBattleMusic;
        bossMusicSource.volume = bossMusicVolume;
        bossMusicSource.loop = true;

        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Pause();
            bgmPausedForBossMusic = true;
        }

        bossMusicSource.Play();
    }

    public void StopBossBattleMusic()
    {
        if (bossMusicSource != null)
            bossMusicSource.Stop();

        if (bgmPausedForBossMusic && bgmSource != null)
            bgmSource.UnPause();

        bgmPausedForBossMusic = false;
    }
}
