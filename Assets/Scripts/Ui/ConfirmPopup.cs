using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ConfirmPopup : BasePopup

{
    [Header("UI")]
   
    public Button confirmButton;
    public Button cancelButton;
    public TMP_Text titleText;
    public TMP_Text messageText;
    public Button backgroundButton;

    private Action onConfirm;
    public Action OnClosed;

    protected override void Awake()
    {
        base.Awake();

        confirmButton.onClick.AddListener(OnConfirmClicked);
        cancelButton.onClick.AddListener(Hide);

        if (backgroundButton != null)
            backgroundButton.onClick.AddListener(Hide);
    }


    public void Show(string title, string message, Action confirmAction)
    {
        titleText.text = title;
        messageText.text = message;
        onConfirm = confirmAction;
    }


    private void OnConfirmClicked()
    {
        onConfirm?.Invoke();
        Hide();
    }

    public override void Hide()
    {
        messageText.text = "";
        onConfirm = null;

        OnClosed?.Invoke();
        base.Hide();
    }

}
