using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Plays the HomeScene intro and enters the game once it finishes.
/// The Start button can still invoke SceneLoader at any time to skip the intro.
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class HomeIntroController : MonoBehaviour
{
    [SerializeField] private GameObject background;
    [SerializeField] private SceneLoader sceneLoader;

    private VideoPlayer videoPlayer;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.loopPointReached += OnIntroFinished;

        // The background image is opaque, so it must not cover the video.
        if (background != null)
            background.SetActive(false);
    }

    private void Start()
    {
        videoPlayer.enabled = true;
        videoPlayer.Play();
    }

    private void OnIntroFinished(VideoPlayer source)
    {
        if (sceneLoader != null)
            sceneLoader.LoadGameScene();
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            // Stop() also stops audio sent directly by the VideoPlayer.
            videoPlayer.Stop();
            videoPlayer.loopPointReached -= OnIntroFinished;
        }
    }
}
