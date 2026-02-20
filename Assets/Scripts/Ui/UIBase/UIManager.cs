using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum PopupType
{
    Shop,
    Inventory,
    Settings,
    Stats,
    Skill,
    ItemConfirm,
    Gacha,
    Hero
}
    
[Serializable]
public class PopupEntry
{
    public PopupType type;
    public BasePopup prefab;
}

public class UIManager : Singleton<UIManager>
{
    [Header("Blur Background")]
    [SerializeField] private GameObject blurBG;

    [Header("Popup Prefabs")]
    [SerializeField] private List<PopupEntry> popupEntries;

    // popupType -> prefab
    private Dictionary<PopupType, BasePopup> popupPrefabDict = new();

    // popupType -> instance đang mở
    private Dictionary<PopupType, BasePopup> activePopups = new();

    protected override void Awake()
    {
        base.Awake();

        popupPrefabDict.Clear();
        activePopups.Clear();

        foreach (var entry in popupEntries)
        {
            if (entry.prefab == null) continue;

            if (!popupPrefabDict.ContainsKey(entry.type))
            {
                popupPrefabDict.Add(entry.type, entry.prefab);
            }
        }

        UpdateBlurState();
    }
    
    private void UpdateTimeScale()
    {
        var invalidKeys = new List<PopupType>();

        foreach (var pair in activePopups)
        {
            if (pair.Value == null)
                invalidKeys.Add(pair.Key);
        }

        foreach (var key in invalidKeys)
            activePopups.Remove(key);

        Time.timeScale = activePopups.Count > 0 ? 0f : 1f;
    }

    // ================== SHOW ==================

   

    // ================== HIDE ==================

    public void HidePopupByType(PopupType type)
    {
        if (!activePopups.TryGetValue(type, out var popup))
            return;

        popup.Hide(); // popup tự Destroy
        activePopups.Remove(type);

        UpdateBlurState();
        UpdateTimeScale();
    }

    public void HideAllPopups()
    {
        foreach (var popup in activePopups.Values)
        {
            if (popup != null)
                popup.Hide();
        }

        activePopups.Clear();
        UpdateBlurState();
        UpdateTimeScale();
    }

    // ================== CHECK ==================

    public bool IsPopupOpen(PopupType type)
    {
        return activePopups.ContainsKey(type);
    }
    
    // ================== BLUR ==================

    public void UpdateBlurState()
    {
        if (blurBG == null) return;

        bool anyPopupOpen = activePopups.Count > 0;
        blurBG.SetActive(anyPopupOpen);

        if (blurBG.TryGetComponent(out CanvasGroup cg))
        {
            cg.DOFade(anyPopupOpen ? 0.5f : 0f, 0.15f)
              .SetUpdate(true);
        }
    }
  
    
    public BasePopup ShowPopupByType(PopupType type)
    {
        if (activePopups.TryGetValue(type, out var existing))
        {
            if (existing != null)
                return existing;

            // popup đã bị destroy → xoá reference cũ
            activePopups.Remove(type);
        }

        if (!popupPrefabDict.TryGetValue(type, out var prefab))
        {
            Debug.LogWarning($"[UIManager] Không tìm thấy prefab cho popup: {type}");
            return null;
        }

        var instance = Instantiate(prefab, transform);
        activePopups[type] = instance;

        instance.Show();
        UpdateBlurState();
        UpdateTimeScale();

        return instance;
    }
    
    public bool TryGetPopup(PopupType type, out BasePopup popup)
    {
        return activePopups.TryGetValue(type, out popup);
    }
    // ================== GACHA ==================

    


}
