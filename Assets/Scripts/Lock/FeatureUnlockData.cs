// FeatureUnlockData.cs
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FeatureUnlockEntry
{
    public FeatureType featureType;
    public int requiredLevel;
    [TextArea] public string lockedMessage;
}

[CreateAssetMenu(fileName = "FeatureUnlockData", menuName = "Config/FeatureUnlockData")]
public class FeatureUnlockData : ScriptableObject
{
    public List<FeatureUnlockEntry> entries;

    public FeatureUnlockEntry Get(FeatureType type)
        => entries.Find(e => e.featureType == type);
}