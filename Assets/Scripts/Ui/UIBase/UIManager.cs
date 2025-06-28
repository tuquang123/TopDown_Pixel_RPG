using System;
using System.Collections.Generic;
using UnityEngine;

public enum PopupType
{
    Inventory,
    PlayerStats,
}

public class UIManager : Singleton<UIManager>
{
    private Dictionary<Type, BasePopup> popupTypeDict = new();

    protected override void Awake()
    {
        base.Awake();
        
        var popups = GetComponentsInChildren<BasePopup>(true);
        foreach (var popup in popups)
        {
            popupTypeDict[popup.GetType()] = popup;
        }
    }
    
    public void ShowPopup<T>() where T : BasePopup
    {
        if (popupTypeDict.TryGetValue(typeof(T), out var popup))
        {
            popup.Show();
        }
        else
        {
            Debug.LogWarning($"[UIManager] Popup {typeof(T)} not found.");
        }
    }

    public void HidePopup<T>() where T : BasePopup
    {
        if (popupTypeDict.TryGetValue(typeof(T), out var popup))
        {
            popup.Hide();
        }
        else
        {
            Debug.LogWarning($"[UIManager] Popup {typeof(T)} not found.");
        }
    }
    
    public T GetPopup<T>() where T : BasePopup
    {
        if (popupTypeDict.TryGetValue(typeof(T), out var popup))
            return popup as T;

        return null;
    }
    
    public void HideAllPopups()
    {
        foreach (var popup in popupTypeDict.Values)
        {
            popup.Hide();
        }
    }
}