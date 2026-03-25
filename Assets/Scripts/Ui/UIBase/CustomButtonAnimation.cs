using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomButtonAnimation : Button
{
    [SerializeField] private Image targetBackgroundImage;
    //[SerializeField] private SOUND_TYPE soundClick = SOUND_TYPE.SFX_SHOP_BUTTON_CLICK;
    [SerializeField] private bool muteDueToTriggeredWithOtherSfx;
    [SerializeField] private bool ignoreNormalScale = true;
    
    private const float PRESSED_DURATION = 0.01f;
    private static readonly Vector3 PRESSED_SIZE = new Vector3(0.97f, 0.97f, 1f);
    private static readonly Color PRESSED_COLOR = new Color(176f/255f, 176f/255f, 176f/255f);
    
    private static readonly Color NORMAL_COLOR = Color.white;
    private static readonly Vector3 NORMAL_SIZE = Vector3.one;

    protected override void Start()
    {
        base.Start();
        transition = Transition.None;
        
#if UNITY_EDITOR
        // Try to add the image on this gameObject as target because it's normally the background
        if (targetBackgroundImage == null)
        {
            targetBackgroundImage = gameObject.GetComponent<Image>();
        }
#endif
    }
    
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || !IsActive() || !IsInteractable())
            return;

        if (!muteDueToTriggeredWithOtherSfx)
        {
            //CBGAudioManager.Instance.PlaySoundEffect(soundClick);
        }
        
        base.OnPointerClick(eventData);
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        switch (state)
        {
            case SelectionState.Normal:
                if (!ignoreNormalScale)
                {
                    ButtonNormal();
                }
                break;
            case SelectionState.Highlighted:
                if (!ignoreNormalScale)
                {
                    ButtonNormal();
                }
                break;
            case SelectionState.Pressed:
                ButtonPressed();
                break;
            case SelectionState.Selected:
                ButtonNormal();
                break;
            case SelectionState.Disabled:
                ButtonNormal();
                break;
            default:
                ButtonNormal();
                break;
        }
    }

    private void ButtonPressed()
    {
        if (targetBackgroundImage != null)
        {
            DOTween.Kill(targetBackgroundImage);
            targetBackgroundImage.transform.DOScale(PRESSED_SIZE, PRESSED_DURATION).SetEase(Ease.OutCubic);
            targetBackgroundImage.DOColor(PRESSED_COLOR, PRESSED_DURATION);
        } else
        {
            DOTween.Kill(transform);
            transform.DOScale(PRESSED_SIZE, PRESSED_DURATION).SetEase(Ease.OutCubic);
        }
    }

    private void ButtonNormal()
    {
        if (targetBackgroundImage != null)
        {
            DOTween.Kill(targetBackgroundImage);
            targetBackgroundImage.transform.DOScale(NORMAL_SIZE, PRESSED_DURATION).SetEase(Ease.OutCubic);
            targetBackgroundImage.DOColor(NORMAL_COLOR, PRESSED_DURATION);
        } else
        {
            DOTween.Kill(transform);
            transform.DOScale(NORMAL_SIZE, PRESSED_DURATION).SetEase(Ease.OutCubic);
        }
    }
}
