using System;
using UnityEngine;

[Serializable]

public class GachaItem
{
    public ItemData item;
    [Range(0f, 100f)]
    public float rate;
}
