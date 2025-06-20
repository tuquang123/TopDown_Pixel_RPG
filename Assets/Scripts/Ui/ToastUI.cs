using TMPro;
using UnityEngine;
using System.Collections;

public class ToastUI : MonoBehaviour, IGameEventListener<string>
{
    [SerializeField] private GameObject toastPanel; 
    [SerializeField] private TextMeshProUGUI toastText;
    [SerializeField] private float showDuration = 2f;

    private Coroutine currentToast;

    private void OnEnable()
    {
        GameEvents.OnShowToast.RegisterListener(this);
    }

    private void OnDisable()
    {
        GameEvents.OnShowToast.UnregisterListener(this);
    }

    public void OnEventRaised(string message)
    {
        if (currentToast != null)
            StopCoroutine(currentToast);

        currentToast = StartCoroutine(ShowToast(message));
    }

    private IEnumerator ShowToast(string message)
    {
        toastText.text = message;
        toastPanel.SetActive(true);

        yield return new WaitForSeconds(showDuration);

        toastPanel.SetActive(false);
    }
}