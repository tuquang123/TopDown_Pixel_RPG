using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadGameScene()
    {
        SceneLoadRequest.TargetScene = "GameScene"; // Gán TÊN scene cần load
        SceneManager.LoadScene("LoadingScene");     // Rồi mới vào scene Loading
    }

    public void LoadScene(string sceneName)
    {
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