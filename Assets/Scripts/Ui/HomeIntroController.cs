using UnityEngine;
using UnityEngine.Video;
using System.IO;

/// <summary>
/// Plays the HomeScene intro and enters the game once it finishes.
/// The Start button can still invoke SceneLoader at any time to skip the intro.
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class HomeIntroController : MonoBehaviour
{
    private const string WebIntroVideoPath = "Intro/videoloding.mp4";

    [SerializeField] private GameObject background;
    [SerializeField] private SceneLoader sceneLoader;

    private VideoPlayer videoPlayer;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.loopPointReached += OnIntroFinished;

        // The background image is opaque, so it must not cover the video.
        if (background != null)
            background.SetActive(false);
    }

    private void Start()
    {
        videoPlayer.enabled = true;

        // Keep the file in StreamingAssets and play it through a URL. WebGL only
        // supports VideoSource.Url, while this also works in the Unity Editor.
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = Path.Combine(Application.streamingAssetsPath, WebIntroVideoPath)
            .Replace("\\", "/");

#if UNITY_WEBGL && !UNITY_EDITOR
        // Browsers block autoplay that starts with audio. The intro remains visible;
        // its audio is intentionally disabled on WebGL.
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
#endif
        videoPlayer.prepareCompleted += PlayPreparedVideo;
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.Prepare();
    }

    private void PlayPreparedVideo(VideoPlayer source)
    {
        source.Play();
    }

    private void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogError($"Could not play the HomeScene intro video: {message}");
        if (background != null)
            background.SetActive(true);
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
            videoPlayer.prepareCompleted -= PlayPreparedVideo;
            videoPlayer.errorReceived -= OnVideoError;
        }
    }
}
