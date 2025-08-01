// riêng cho ngựa

using UnityEngine;

[CreateAssetMenu(fileName = "NewHorseData", menuName = "Mount/Horse Data")]
public class HorseData : ScriptableObject
{
    public Sprite ACC;
    public Sprite BodyBack;
    public Sprite BodyFront;
    public Sprite FootBackBottom;
    public Sprite FootBackTop;
    public Sprite FootFrontBottom;
    public Sprite FootFrontTop;
    public Sprite Head;
    public Sprite Neck;
    public Sprite Tail;

    public Color color = Color.white;
}