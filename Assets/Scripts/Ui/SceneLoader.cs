using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private bool isLoading;

    public void LoadGameScene()
    {
        if (isLoading)
            return;

        isLoading = true;
        SceneLoadRequest.TargetScene = "GameScene"; // Gán TÊN scene cần load
        SceneManager.LoadScene("LoadingScene");     // Rồi mới vào scene Loading
    }

    public void LoadScene(string sceneName)
    {
        if (isLoading)
            return;

        isLoading = true;
        SceneLoadRequest.TargetScene = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
