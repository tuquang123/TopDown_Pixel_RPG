using UnityEngine;
using UnityEngine.UI;

public class LevelBackTrigger : MonoBehaviour
{
    [SerializeField] private float delay = 1.5f;
    [SerializeField] private Slider loadingBar;

    private float timer;
    private bool playerInside;
    private bool triggered;

    void Start()
    {
        if (loadingBar != null)
        {
            loadingBar.value = 0f;
            loadingBar.gameObject.SetActive(false); // Ẩn lúc đầu
        }
    }

    void Update()
    {
        if (!playerInside || triggered) return;

        timer += Time.deltaTime;

        if (loadingBar != null)
        {
            loadingBar.gameObject.SetActive(true); // Hiện khi bắt đầu load
            loadingBar.value = timer / delay;
        }

        if (timer >= delay)
        {
            triggered = true;

            if (LevelManager.Instance != null)
                LevelManager.Instance.PreviousLevel();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        timer = 0f;

        if (loadingBar != null)
        {
            loadingBar.value = 0f;
            loadingBar.gameObject.SetActive(false); // Ẩn khi ra ngoài
        }
    }
}