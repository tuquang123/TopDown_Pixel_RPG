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

    void Start()
    {
        // Reset loading bar
        if (loadingBar != null)
        {
            loadingBar.value = 0f;
            loadingBar.gameObject.SetActive(false);
        }

        // Khi start scene, update text dựa trên trigger
        UpdateMapNameForTrigger();
    }

    void Update()
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

    private void OnTriggerEnter2D(Collider2D other)
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

        // Cập nhật text ngay khi vào trigger dựa theo loại Next/Back
        UpdateMapNameForTrigger();
    }

    private void OnTriggerExit2D(Collider2D other)
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

    /// <summary>
    /// Cập nhật tên map dựa trên trigger type
    /// </summary>
    private void UpdateMapNameForTrigger()
    {
        if (mapNameText == null || LevelManager.Instance == null) return;

        var db = LevelManager.Instance.levelDatabase;
        int index = LevelManager.Instance.CurrentLevel;

        if (triggerType == TriggerType.Next)
            index += 1; // hiển thị map tiếp theo
        else
            index -= 1; // hiển thị map trước

        // Chống vượt quá bounds
        if (index < 0) index = 0;
        if (index >= db.TotalLevels) index = db.TotalLevels - 1;

        var level = db.GetLevel(index);
        if (level == null) return;

        mapNameText.text = level.levelName;
        Canvas.ForceUpdateCanvases(); // đảm bảo TMP update ngay
    }
    public TriggerType Type => triggerType;
    
}