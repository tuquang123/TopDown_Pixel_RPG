using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(
    fileName = "ItemTierColorConfig",
    menuName = "Game Config/Item Tier Color Config"
)]
public class ItemTierColorConfig : ScriptableObject
{
    [Serializable]
    public struct TierColor
    {
        public ItemTier tier;
        public Color color;
        public Sprite background;
    }

    [SerializeField]
    private List<TierColor> tierColors = new();

    private Dictionary<ItemTier, Color> cache;

    public Color GetColor(ItemTier tier)
    {
        if (cache == null)
            BuildCache();

        return cache != null && cache.TryGetValue(tier, out var color)
            ? color
            : Color.white;
    }

    private void BuildCache()
    {
        cache = new Dictionary<ItemTier, Color>();
        foreach (var entry in tierColors)
        {
            if (!cache.ContainsKey(entry.tier))
                cache.Add(entry.tier, entry.color);
        }
    }
    public Sprite GetBackground(ItemTier tier)
    {
        if (cache == null)
            BuildCache();

        return cache != null && cache.TryGetValue(tier, out var color)
            ? tierColors.Find(x => x.tier == tier).background
            : null;
    }
     
}