using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class StartScene : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Animator fallbackAnimator;
    [SerializeField] private Image fallbackImage;
    [SerializeField] private AspectRatioFitter fallbackAspectFitter;
    [SerializeField] private string nextSceneName = "MainScene";
    [SerializeField] private float prepareTimeoutSeconds = 3f;

    private bool _isLoading;
    private bool _isFallbackPlaying;
    private CancellationTokenSource _prepareCts;

    private void Reset()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        fallbackAnimator = GetComponent<Animator>();
        fallbackImage = GetComponent<Image>();
        fallbackAspectFitter = GetComponent<AspectRatioFitter>();
    }

    private void Awake()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (fallbackAnimator == null)
        {
            fallbackAnimator = GetComponent<Animator>();
        }

        if (fallbackImage == null)
        {
            fallbackImage = GetComponent<Image>();
        }

        if (fallbackAspectFitter == null)
        {
            fallbackAspectFitter = GetComponent<AspectRatioFitter>();
        }

        ShowFallbackBackground();
    }

    private void OnEnable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.errorReceived += OnVideoErrorReceived;
        }
    }

    private void OnDisable()
    {
        CancelPrepareTask();

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.errorReceived -= OnVideoErrorReceived;
        }
    }

    private void Start()
    {
        if (videoPlayer == null)
        {
            PlayFallbackAnimation();
            return;
        }

        if (videoPlayer.clip == null && string.IsNullOrWhiteSpace(videoPlayer.url))
        {
            Debug.LogWarning("StartScene VideoPlayer has no clip or url. Playing fallback intro animation.");
            PlayFallbackAnimation();
            return;
        }

        CancelPrepareTask();
        _prepareCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        PrepareAndPlayAsync(_prepareCts.Token).Forget();
    }

    public void OnLogoFinish()
    {
        LoadMainScene();
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        CancelPrepareTask();

        long lastFrame = (long)source.frameCount - 1;
        if (lastFrame >= 0)
        {
            source.frame = lastFrame;
        }
        source.Pause();
        LoadMainScene();
    }

    private void OnVideoErrorReceived(VideoPlayer source, string message)
    {
        Debug.LogWarning($"Intro video failed: {message}. Playing fallback intro animation.");
        CancelPrepareTask();
        PlayFallbackAnimation();
    }

    private async UniTaskVoid PrepareAndPlayAsync(CancellationToken ct)
    {
        try
        {
            videoPlayer.Prepare();

            int completedTaskIndex = await UniTask.WhenAny(
                UniTask.WaitUntil(
                    () => _isLoading || videoPlayer == null || videoPlayer.isPrepared || videoPlayer.isPlaying,
                    cancellationToken: ct),
                UniTask.Delay(TimeSpan.FromSeconds(prepareTimeoutSeconds), cancellationToken: ct));

            if (completedTaskIndex == 1)
            {
                if (!_isLoading && videoPlayer != null && !videoPlayer.isPrepared && !videoPlayer.isPlaying)
                {
                    Debug.LogWarning($"Intro video prepare timed out after {prepareTimeoutSeconds:0.##} seconds. Playing fallback intro animation.");
                    PlayFallbackAnimation();
                }

                return;
            }

            if (_isLoading || videoPlayer == null || videoPlayer.isPlaying)
            {
                return;
            }

            HideFallbackVisual();
            videoPlayer.Play();
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void PlayFallbackAnimation()
    {
        if (_isLoading || _isFallbackPlaying)
        {
            return;
        }

        if (fallbackAnimator == null || fallbackImage == null)
        {
            Debug.LogWarning("Fallback intro animation is not configured. Skipping to main scene.");
            LoadMainScene();
            return;
        }

        CancelPrepareTask();
        _isFallbackPlaying = true;

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.enabled = false;
        }

        ShowFallbackBackground();
        fallbackAnimator.enabled = true;
        fallbackAnimator.Rebind();
        fallbackAnimator.Play(0, 0, 0f);
        fallbackAnimator.Update(0f);
        ConfigureFallbackLayout();
    }

    private void ConfigureFallbackLayout()
    {
        if (fallbackImage == null)
        {
            return;
        }

        fallbackImage.preserveAspect = true;

        if (fallbackAspectFitter == null || fallbackImage.sprite == null)
        {
            return;
        }

        Rect spriteRect = fallbackImage.sprite.rect;
        if (spriteRect.height <= 0f)
        {
            return;
        }

        fallbackAspectFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fallbackAspectFitter.aspectRatio = spriteRect.width / spriteRect.height;
    }

    private void ShowFallbackBackground()
    {
        if (fallbackImage == null)
        {
            return;
        }

        if (fallbackAnimator != null)
        {
            fallbackAnimator.enabled = true;
            fallbackAnimator.Rebind();
            fallbackAnimator.Play(0, 0, 0f);
            fallbackAnimator.Update(0f);
            fallbackAnimator.enabled = false;
        }

        fallbackImage.enabled = true;
        ConfigureFallbackLayout();
    }

    private void HideFallbackVisual()
    {
        if (fallbackImage != null)
        {
            fallbackImage.enabled = false;
        }

        if (fallbackAnimator != null)
        {
            fallbackAnimator.enabled = false;
        }
    }

    private void CancelPrepareTask()
    {
        _prepareCts?.Cancel();
        _prepareCts?.Dispose();
        _prepareCts = null;
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
