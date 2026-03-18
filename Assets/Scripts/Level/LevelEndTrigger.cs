using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelEndTrigger : MonoBehaviour
{
    [SerializeField] private float delay = 1.5f;
    [SerializeField] private Slider loadingBar;

    [Header("Map UI")]
    [SerializeField] private GameObject mapNameBG;
    [SerializeField] private TMP_Text mapNameText;

    private float timer;
    private bool playerInside;
    private bool triggered;

    void Start()
    {
        if (loadingBar != null)
        {
            loadingBar.value = 0f;
            loadingBar.gameObject.SetActive(false);
        }

        if (mapNameBG != null)
            mapNameBG.SetActive(false);
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

            if (LevelManager.Instance != null)
                LevelManager.Instance.NextLevel();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;

        if (LevelManager.Instance == null || mapNameText == null) return;

        var db = LevelManager.Instance.levelDatabase;
        int nextIndex = LevelManager.Instance.CurrentLevel + 1;

        if (nextIndex >= db.TotalLevels) return;

        var level = db.GetLevel(nextIndex);

        mapNameText.text = level.levelName;   // chỉ hiển thị tên map
        mapNameBG.SetActive(true);
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

        if (mapNameBG != null)
            mapNameBG.SetActive(false);
    }
}