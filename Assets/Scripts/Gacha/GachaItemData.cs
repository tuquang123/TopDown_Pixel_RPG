using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "GachaItemData",
    menuName = "Gacha/Gacha Item Data"
)]
public class GachaItemData : ScriptableObject
{
    public List<GachaItem> items;
}