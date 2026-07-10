using System;
using UnityEngine;

public enum BossSoundEvent
{
    Spawn,
    Attack,
    HitPlayer,
    Hurt,
    Death,
    Summon,
    Dash,
    Shoot
}

[Serializable]
public class BossSoundClipEntry
{
    public string resourcesPath;
    [Range(0f, 2f)] public float volume = 1f;
    [Range(0.25f, 3f)] public float pitchMin = 1f;
    [Range(0.25f, 3f)] public float pitchMax = 1f;
}

[Serializable]
public class BossSoundEventEntry
{
    public string eventType;
    public BossSoundClipEntry[] clips;
}

[Serializable]
public class BossSoundProfile
{
    public string bossId = "default";
    public BossSoundEventEntry[] events;
}

[Serializable]
public class BossSoundLibrary
{
    public BossSoundProfile[] profiles;
}

public static class BossSoundDatabase
{
    private const string DataPath = "Audio/Boss/BossSoundData";
    private static BossSoundLibrary library;

    public static void Play(string bossId, BossSoundEvent eventType)
    {
        BossSoundClipEntry entry = PickClip(bossId, eventType);
        if (entry == null)
            return;

        AudioClip clip = Resources.Load<AudioClip>(entry.resourcesPath);
        if (clip == null)
        {
            Debug.LogWarning($"Boss SFX '{entry.resourcesPath}' không tìm thấy trong Resources.");
            return;
        }

        float pitch = UnityEngine.Random.Range(entry.pitchMin, entry.pitchMax);
        AudioManager.Instance?.PlaySFX(clip, entry.volume, pitch);
    }

    private static BossSoundClipEntry PickClip(string bossId, BossSoundEvent eventType)
    {
        EnsureLoaded();
        BossSoundProfile profile = FindProfile(bossId) ?? FindProfile("default");
        if (profile?.events == null)
            return null;

        foreach (BossSoundEventEntry eventEntry in profile.events)
        {
            if (!string.Equals(eventEntry.eventType, eventType.ToString(), StringComparison.OrdinalIgnoreCase)
                || eventEntry.clips == null
                || eventEntry.clips.Length == 0)
                continue;

            return eventEntry.clips[UnityEngine.Random.Range(0, eventEntry.clips.Length)];
        }

        return null;
    }

    private static BossSoundProfile FindProfile(string bossId)
    {
        if (library?.profiles == null)
            return null;

        foreach (BossSoundProfile profile in library.profiles)
        {
            if (string.Equals(profile.bossId, bossId, StringComparison.OrdinalIgnoreCase))
                return profile;
        }

        return null;
    }

    private static void EnsureLoaded()
    {
        if (library != null)
            return;

        TextAsset json = Resources.Load<TextAsset>(DataPath);
        if (json == null)
        {
            Debug.LogWarning($"Không tìm thấy boss sound data tại Resources/{DataPath}.json");
            library = new BossSoundLibrary();
            return;
        }

        library = JsonUtility.FromJson<BossSoundLibrary>(json.text) ?? new BossSoundLibrary();
    }
}
