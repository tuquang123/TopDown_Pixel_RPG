using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using TMPro;

public static class SceneLoadRequest
{
    public static string TargetScene;
}

public class LoadingScreenController : MonoBehaviour
{
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private TextMeshProUGUI loadingText;

    private string sceneToLoad;

    void Start()
    {
        // Lấy tên scene cần load từ GameManager hoặc static var
        sceneToLoad = SceneLoadRequest.TargetScene;
        LoadSceneAsync().Forget();
    }

    private async UniTaskVoid LoadSceneAsync()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            //Debug.LogError("Scene name is null or empty!");
            return;
        }

        var operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;

        // Loading đến 90%
        while (operation.progress < 0.9f)
        {
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);
            loadingSlider.value = targetProgress;
            loadingText.text = $"{(targetProgress * 100):F0}%";
            await UniTask.Yield();
        }

        // Giữ ở 90% trong 1.5s tạo cảm giác xử lý
        loadingSlider.value = 0.9f;
        loadingText.text = "90%";
        await UniTask.Delay(1500); // delay giữ lại

        // Sau đó từ từ tăng lên 100%
        float timer = 0f;
        while (timer < 0.5f)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Lerp(0.9f, 1f, timer / 0.5f);
            loadingSlider.value = progress;
            loadingText.text = $"{(progress * 100):F0}%";
            await UniTask.Yield();
        }

        loadingSlider.value = 1f;
        loadingText.text = "100%";

        await UniTask.Delay(300); // delay nhỏ cuối cùng

        operation.allowSceneActivation = true;
    }

}