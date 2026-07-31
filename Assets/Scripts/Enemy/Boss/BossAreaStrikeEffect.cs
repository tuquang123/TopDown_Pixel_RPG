using System.Collections;
using UnityEngine;

public enum BossAreaStrikeVisual
{
    Lightning,
    StoneSpike
}

public class BossAreaStrikeEffect : MonoBehaviour
{
    [SerializeField] private float radius = 1.1f;
    [SerializeField] private float warningTime = 0.65f;
    [SerializeField] private float lifeTime = 0.35f;
    [SerializeField] private int damage = 50;
    [SerializeField] private BossAreaStrikeVisual visual = BossAreaStrikeVisual.Lightning;

    private static Sprite circleSprite;
    private static Sprite spikeSprite;

    public void Initialize(float hitRadius, float delay, float stayTime, int hitDamage, BossAreaStrikeVisual strikeVisual)
    {
        radius = hitRadius;
        warningTime = delay;
        lifeTime = stayTime;
        damage = hitDamage;
        visual = strikeVisual;
        StopAllCoroutines();
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        SpriteRenderer warning = CreateSprite("Warning", GetCircleSprite(), new Color(1f, 0.15f, 0.08f, 0.28f), -1);
        warning.transform.localScale = Vector3.one * radius * 2f;

        yield return new WaitForSeconds(warningTime);

        if (warning != null)
            Destroy(warning.gameObject);

        if (visual == BossAreaStrikeVisual.Lightning)
            CreateLightningVisual();
        else
            CreateStoneSpikeVisual();

        DealDamage();
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }

    private void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D hit in hits)
        {
            if (hit == null || hit.TryGetComponent(out EnemyAI _))
                continue;

            if (hit.TryGetComponent(out IDamageable damageable))
                damageable.TakeDamage(damage);
        }
    }

    private void CreateLightningVisual()
    {
        GameObject bolt = new GameObject("LightningBolt");
        bolt.transform.SetParent(transform, false);
        bolt.transform.localPosition = Vector3.zero;

        LineRenderer line = bolt.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.positionCount = 7;
        line.startWidth = 0.08f;
        line.endWidth = 0.03f;
        line.sortingOrder = 80;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = new Color(0.45f, 0.9f, 1f, 1f);
        line.endColor = Color.white;

        for (int i = 0; i < line.positionCount; i++)
        {
            float t = i / (line.positionCount - 1f);
            float x = Random.Range(-0.18f, 0.18f);
            float y = Mathf.Lerp(2.2f, -0.35f, t);
            line.SetPosition(i, new Vector3(x, y, 0f));
        }

        SpriteRenderer flash = CreateSprite("LightningFlash", GetCircleSprite(), new Color(0.55f, 0.9f, 1f, 0.45f), 70);
        flash.transform.localScale = Vector3.one * radius * 1.5f;
    }

    private void CreateStoneSpikeVisual()
    {
        int count = 5;
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / count;
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius * 0.35f;
            SpriteRenderer spike = CreateSprite("StoneSpike", GetSpikeSprite(), new Color(0.72f, 0.68f, 0.58f, 1f), 70);
            spike.transform.localPosition = offset;
            spike.transform.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg - 90f);
            spike.transform.localScale = new Vector3(0.55f, 0.85f, 1f);
        }
    }

    private SpriteRenderer CreateSprite(string objectName, Sprite sprite, Color color, int sortingOrder)
    {
        GameObject obj = new GameObject(objectName);
        obj.transform.SetParent(transform, false);
        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    private static Sprite GetCircleSprite()
    {
        if (circleSprite != null)
            return circleSprite;

        Texture2D texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f)) / 15.5f;
                float alpha = distance <= 1f ? Mathf.Clamp01(1f - distance * 0.35f) : 0f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.Apply();
        circleSprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
        return circleSprite;
    }

    private static Sprite GetSpikeSprite()
    {
        if (spikeSprite != null)
            return spikeSprite;

        Texture2D texture = new Texture2D(16, 24, TextureFormat.RGBA32, false);
        for (int y = 0; y < 24; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                float halfWidth = Mathf.Lerp(1f, 7.5f, y / 23f);
                bool inside = Mathf.Abs(x - 7.5f) <= halfWidth;
                texture.SetPixel(x, y, inside ? Color.white : Color.clear);
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.Apply();
        spikeSprite = Sprite.Create(texture, new Rect(0, 0, 16, 24), new Vector2(0.5f, 0f), 24f);
        return spikeSprite;
    }
}
