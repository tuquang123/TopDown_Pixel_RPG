using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIShine : MonoBehaviour
{
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private float range = 1.5f;
    
    private Material mat;
    private int shineOffsetID;
    private float offset;

    void Awake()
    {
        // Cache material và property ID
        Image img = GetComponent<Image>();
        mat = Instantiate(img.material);
        img.material = mat;
        shineOffsetID = Shader.PropertyToID("_ShineOffset");
    }

    void Update()
    {
        offset = Mathf.Repeat(offset + Time.deltaTime * speed, range * 2f) - range;
        mat.SetFloat(shineOffsetID, offset);
    }

    void OnDestroy()
    {
        if (mat != null)
            Destroy(mat); // tránh leak material runtime
    }
}