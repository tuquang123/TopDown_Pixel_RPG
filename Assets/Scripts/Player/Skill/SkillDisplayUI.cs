using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class SkillDisplayUI : MonoBehaviour
{
    [Header("UI Refs")]
    public Image backgroundImage;
    public Image skillIcon;
    public Image selectedOverlay;       // ← Image riêng để highlight, bật/tắt khi chọn
    public TextMeshProUGUI skillName;
    public TextMeshProUGUI skillDesc;
    public Button selectButton;

    [Header("Overlay Settings")]
    public float pulseSpeed    = 3f;
    public float pulseMinAlpha = 0.3f;
    public float pulseMaxAlpha = 0.85f;

    private Coroutine _pulseCoroutine;
    private int       _index;
    private Action<int> _onClickCallback;

    // ====================== INIT ======================
    private void Awake()
    {
        if (selectButton != null)
            selectButton.onClick.AddListener(() => _onClickCallback?.Invoke(_index));

        // Tắt overlay ngay từ đầu
        SetOverlayActive(false);
    }

    // ====================== DISPLAY ======================
    public void DisplaySkill(SkillData data, int index, Action<int> onClick)
    {
        _index           = index;
        _onClickCallback = onClick;

        if (skillName != null) skillName.text = data.skillName;
        if (skillDesc != null) skillDesc.text = data.GetDescriptionAtLevel(1);

        if (skillIcon != null && data.icon != null)
            skillIcon.sprite = data.icon;

        // Màu overlay theo rarity/type
        if (selectedOverlay != null)
        {
            Color c = data.GetRarityColor();
            c.a = 0f;
            selectedOverlay.color = c;
        }

        // Reset sạch trạng thái cũ
        ForceReset();
    }

    // ====================== SELECT ======================
    public void SetSelected(bool selected)
    {
        // Dừng coroutine cũ dù selected hay không
        StopPulse();

        if (selected)
        {
            SetOverlayActive(true);
            _pulseCoroutine      = StartCoroutine(PulseOverlay());
            transform.localScale = Vector3.one * 1.05f;
        }
        else
        {
            ForceReset();
        }
    }

    // ====================== FORCE RESET ======================
    /// Gọi khi reroll hoặc show popup mới — đảm bảo sạch hoàn toàn
    public void ForceReset()
    {
        StopPulse();
        SetOverlayActive(false);
        transform.localScale = Vector3.one;
    }

    // ====================== HELPERS ======================
    private void StopPulse()
    {
        if (_pulseCoroutine != null)
        {
            StopCoroutine(_pulseCoroutine);
            _pulseCoroutine = null;
        }
    }

    private void SetOverlayActive(bool active)
    {
        if (selectedOverlay == null) return;

        // Bật/tắt overlay bằng alpha thay vì SetActive (tránh layout jump)
        Color c = selectedOverlay.color;
        c.a = active ? pulseMinAlpha : 0f;
        selectedOverlay.color = c;
        selectedOverlay.gameObject.SetActive(active);
    }

    // ====================== COROUTINE ======================
    private IEnumerator PulseOverlay()
    {
        float t = 0f;

        while (true)
        {
            t += Time.unscaledDeltaTime * pulseSpeed;
            float alpha = Mathf.Lerp(pulseMinAlpha, pulseMaxAlpha,
                                     (Mathf.Sin(t) + 1f) * 0.5f);

            if (selectedOverlay != null)
            {
                Color c = selectedOverlay.color;
                c.a = alpha;
                selectedOverlay.color = c;
            }

            yield return null;
        }
    }
}