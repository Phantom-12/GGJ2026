using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class StartScene : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string nextSceneName = "MainScene";

    private bool _isLoading;

    private void Reset()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    private void Awake()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }
    }

    private void OnEnable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }

    private void Start()
    {
        if (videoPlayer != null && !videoPlayer.isPlaying)
        {
            videoPlayer.Play();
        }
    }

    public void OnLogoFinish()
    {
        LoadMainScene();
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        long lastFrame = (long)source.frameCount - 1;
        if (lastFrame >= 0)
        {
            source.frame = lastFrame;
        }
        source.Pause();
        LoadMainScene();
    }

    private void LoadMainScene()
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}
