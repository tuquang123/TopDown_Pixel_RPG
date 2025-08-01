using UnityEngine;

public class HorseRenderer : MonoBehaviour
{
    [Header("Parts")]
    public SpriteRenderer ACC;
    public SpriteRenderer BodyBack;
    public SpriteRenderer BodyFront;
    public SpriteRenderer FootBackBottom;
    public SpriteRenderer FootBackTop;
    public SpriteRenderer FootFrontBottom;
    public SpriteRenderer FootFrontTop;
    public SpriteRenderer Head;
    public SpriteRenderer Neck;
    public SpriteRenderer Tail;

    public void ApplyHorseData(HorseData data)
    {
        if (data == null) return;

        ACC.sprite = data.ACC;
        BodyBack.sprite = data.BodyBack;
        BodyFront.sprite = data.BodyFront;
        FootBackBottom.sprite = data.FootBackBottom;
        FootBackTop.sprite = data.FootBackTop;
        FootFrontBottom.sprite = data.FootFrontBottom;
        FootFrontTop.sprite = data.FootFrontTop;
        Head.sprite = data.Head;
        Neck.sprite = data.Neck;
        Tail.sprite = data.Tail;

        SetColor(data.color);
    }

    public void SetColor(Color c)
    {
        ACC.color = c;
        BodyBack.color = c;
        BodyFront.color = c;
        FootBackBottom.color = c;
        FootBackTop.color = c;
        FootFrontBottom.color = c;
        FootFrontTop.color = c;
        Head.color = c;
        Neck.color = c;
        Tail.color = c;
    }

    public void Clear()
    {
        ACC.sprite = null;
        BodyBack.sprite = null;
        BodyFront.sprite = null;
        FootBackBottom.sprite = null;
        FootBackTop.sprite = null;
        FootFrontBottom.sprite = null;
        FootFrontTop.sprite = null;
        Head.sprite = null;
        Neck.sprite = null;
        Tail.sprite = null;
    }
}
