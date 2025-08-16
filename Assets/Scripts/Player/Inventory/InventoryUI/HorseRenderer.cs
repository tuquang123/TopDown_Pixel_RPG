using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct HorsePart
{
    public string name;
    public SpriteRenderer renderer;
}

[RequireComponent(typeof(Transform))]
public class HorseRenderer : MonoBehaviour
{
    [Header("Core Parts")]
    public HorsePart ACC;
    public HorsePart BodyBack;
    public HorsePart BodyFront;
    public HorsePart Head;
    public HorsePart Neck;
    public HorsePart Tail;

    [Header("Legs (Primary)")]
    public HorsePart FootBackBottom;
    public HorsePart FootBackTop;
    public HorsePart FootFrontBottom;
    public HorsePart FootFrontTop;

    [Header("Legs (Secondary)")]
    public HorsePart FootBackBottom2;
    public HorsePart FootBackTop2;
    public HorsePart FootFrontBottom2;
    public HorsePart FootFrontTop2;

    // Cached for iteration
    private IEnumerable<HorsePart> AllParts()
    {
        yield return ACC;
        yield return BodyBack;
        yield return BodyFront;
        yield return Head;
        yield return Neck;
        yield return Tail;
        yield return FootBackBottom;
        yield return FootBackTop;
        yield return FootFrontBottom;
        yield return FootFrontTop;
        yield return FootBackBottom2;
        yield return FootBackTop2;
        yield return FootFrontBottom2;
        yield return FootFrontTop2;
    }

    public void ApplyHorseData(HorseData data)
    {
        if (data == null) return;

        SafeSet(ACC, data.ACC);
        SafeSet(BodyBack, data.BodyBack);
        SafeSet(BodyFront, data.BodyFront);
        SafeSet(FootBackBottom, data.FootBackBottom);
        SafeSet(FootBackTop, data.FootBackTop);
        SafeSet(FootFrontBottom, data.FootFrontBottom);
        SafeSet(FootFrontTop, data.FootFrontTop);

        // secondary legs mirror primary if not separately provided
        SafeSet(FootBackBottom2, data.FootBackBottom);
        SafeSet(FootBackTop2, data.FootBackTop);
        SafeSet(FootFrontBottom2, data.FootFrontBottom);
        SafeSet(FootFrontTop2, data.FootFrontTop);

        SafeSet(Head, data.Head);
        SafeSet(Neck, data.Neck);
        SafeSet(Tail, data.Tail);

        SetColor(data.color);
    }

    private void SafeSet(HorsePart part, Sprite sprite)
    {
        if (part.renderer != null)
            part.renderer.sprite = sprite;
    }

    public void SetColor(Color c)
    {
        foreach (var part in AllParts())
        {
            if (part.renderer != null)
                part.renderer.color = c;
        }
    }

    public void Clear()
    {
        foreach (var part in AllParts())
        {
            if (part.renderer != null)
                part.renderer.sprite = null;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // tự động đặt tên cho helper nếu trống để dễ debug
        AssignDefaultNames(ref ACC, "ACC");
        AssignDefaultNames(ref BodyBack, "BodyBack");
        AssignDefaultNames(ref BodyFront, "BodyFront");
        AssignDefaultNames(ref FootBackBottom, "FootBackBottom");
        AssignDefaultNames(ref FootBackTop, "FootBackTop");
        AssignDefaultNames(ref FootFrontBottom, "FootFrontBottom");
        AssignDefaultNames(ref FootFrontTop, "FootFrontTop");
        AssignDefaultNames(ref FootBackBottom2, "FootBackBottom2");
        AssignDefaultNames(ref FootBackTop2, "FootBackTop2");
        AssignDefaultNames(ref FootFrontBottom2, "FootFrontBottom2");
        AssignDefaultNames(ref FootFrontTop2, "FootFrontTop2");
        AssignDefaultNames(ref Head, "Head");
        AssignDefaultNames(ref Neck, "Neck");
        AssignDefaultNames(ref Tail, "Tail");
    }

    private void AssignDefaultNames(ref HorsePart part, string defaultName)
    {
        if (string.IsNullOrEmpty(part.name))
            part.name = defaultName;
    }

    // Ghi chú các phần thiếu khi chạy trong editor
    private void OnDrawGizmos()
    {
        foreach (var part in AllParts())
        {
            if (part.renderer == null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(transform.position, 0.1f);
                UnityEditor.Handles.Label(transform.position + Vector3.up * 0.2f, $"Missing: {part.name}");
            }
        }
    }
#endif
}
