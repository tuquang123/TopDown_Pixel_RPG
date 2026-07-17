using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelTrigger : MonoBehaviour
{
    public enum TriggerType { Next, Back }

    [Header("Trigger Settings")]
    [SerializeField] private TriggerType triggerType = TriggerType.Next;
    [SerializeField] private float delay = 1.5f;
    [SerializeField] private Slider loadingBar;

    [Header("Map UI")]
    [SerializeField] private TMP_Text mapNameText;

    private float timer;
    private bool playerInside;
    private bool triggered;

    protected virtual void Start()
    {
        if (loadingBar != null)
        {
            loadingBar.value = 0f;
            loadingBar.gameObject.SetActive(false);
        }

        UpdateMapNameForTrigger();
    }

    protected virtual void Update()
    {
        if (!playerInside || triggered) return;

        timer += Time.deltaTime;

        if (loadingBar != null)
        {
            loadingBar.gameObject.SetActive(true);
            loadingBar.value = timer / delay;
        }

        if (timer >= delay)
        {
            triggered = true;

            if (LevelManager.Instance == null) return;

            if (triggerType == TriggerType.Next)
                LevelManager.Instance.NextLevel();
            else
                LevelManager.Instance.PreviousLevel();
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;
        timer = 0f;
        triggered = false;

        if (loadingBar != null)
        {
            loadingBar.value = 0f;
            loadingBar.gameObject.SetActive(true);
        }

        UpdateMapNameForTrigger();
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        timer = 0f;
        triggered = false;

        if (loadingBar != null)
        {
            loadingBar.value = 0f;
            loadingBar.gameObject.SetActive(false);
        }
    }

    private void UpdateMapNameForTrigger()
    {
        if (mapNameText == null || LevelManager.Instance == null) return;

        var db = LevelManager.Instance.levelDatabase;
        int index = LevelManager.Instance.CurrentLevel;

        if (triggerType == TriggerType.Next)
            index += 1;
        else
            index -= 1;

        if (index < 0) index = 0;
        if (index >= db.TotalLevels) index = db.TotalLevels - 1;

        var level = db.GetLevel(index);
        if (level == null) return;

        mapNameText.text = level.levelName;
        Canvas.ForceUpdateCanvases();
    }

    public TriggerType Type => triggerType;
}
