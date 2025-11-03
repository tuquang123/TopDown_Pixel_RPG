using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    private Dictionary<Type, BasePopup> popupTypeDict = new();
    [SerializeField] private GameObject blurBG;
    
    protected override void Awake()
    {
        base.Awake();
        
        var popups = GetComponentsInChildren<BasePopup>(true);
        foreach (var popup in popups)
        {
            popupTypeDict[popup.GetType()] = popup;
        }
    }
    public void UpdateBlurState()
    {
        if (blurBG == null) return;

        bool anyActive = false;
        foreach (var popup in popupTypeDict.Values)
        {
            if (popup.gameObject.activeSelf)
            {
                anyActive = true;
                break;
            }
        }

        blurBG.SetActive(anyActive);
        
         if (blurBG.TryGetComponent(out CanvasGroup cg))
         {
             cg.DOFade(anyActive ? .5f : 0f, 0.15f);
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