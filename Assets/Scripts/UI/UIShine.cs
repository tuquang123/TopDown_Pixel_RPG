using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIShineEffect : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float minOffset = -1.5f;
    [SerializeField] private float maxOffset = 1.5f;

    private Material mat;
    private int shineOffsetID;
    private float offset;

    void Awake()
    {
        Image img = GetComponent<Image>();

        // Tạo instance riêng cho material
        mat = Instantiate(img.material);
        img.material = mat;

        // Cache property ID (hiệu năng tốt hơn)
        shineOffsetID = Shader.PropertyToID("_ShineLocation"); // hoặc "_ShineOffset" tùy shader bạn có
    }

    void Update()
    {
        offset += Time.deltaTime * speed;
        if (offset > maxOffset) offset = minOffset;
        mat.SetFloat(shineOffsetID, offset);
    }

    void OnDestroy()
    {
        if (mat != null)
            Destroy(mat);
    }
}