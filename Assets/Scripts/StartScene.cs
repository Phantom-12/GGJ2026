using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Image))]
public class StartScene : MonoBehaviour
{
    [SerializeField] private Animator introAnimator;
    [SerializeField] private Image introImage;
    [SerializeField] private AspectRatioFitter introAspectFitter;
    [SerializeField] private AudioClip introSfx1;
    [SerializeField] private AudioClip introSfx2;
    [SerializeField] [Range(0f, 1f)] private float introSfxVolume = 1f;
    [SerializeField] private string nextSceneName = "MainScene";

    private bool _isLoading;

    private void Reset()
    {
        introAnimator = GetComponent<Animator>();
        introImage = GetComponent<Image>();
        introAspectFitter = GetComponent<AspectRatioFitter>();
    }

    private void Awake()
    {
        if (introAnimator == null)
        {
            introAnimator = GetComponent<Animator>();
        }

        if (introImage == null)
        {
            introImage = GetComponent<Image>();
        }

        if (introAspectFitter == null)
        {
            introAspectFitter = GetComponent<AspectRatioFitter>();
        }
    }

    private void Start()
    {
        PlayIntroAnimation();
    }

    public void OnLogoFinish()
    {
        LoadMainScene();
    }

    public void PlayIntroSfx1()
    {
        PlayIntroSfx(introSfx1);
    }

    public void PlayIntroSfx2()
    {
        PlayIntroSfx(introSfx2);
    }

    private void PlayIntroAnimation()
    {
        if (introAnimator == null || introImage == null)
        {
            Debug.LogWarning("StartScene intro animation is not configured. Skipping to main scene.");
            LoadMainScene();
            return;
        }

        introImage.enabled = true;
        ConfigureIntroLayout();

        introAnimator.enabled = true;
        introAnimator.Rebind();
        introAnimator.Play(0, 0, 0f);
        introAnimator.Update(0f);
    }

    private void PlayIntroSfx(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        Vector3 playPosition = Camera.main != null ? Camera.main.transform.position : transform.position;
        AudioSource.PlayClipAtPoint(clip, playPosition, Mathf.Clamp01(introSfxVolume));
    }

    private void ConfigureIntroLayout()
    {
        if (introImage == null)
        {
            return;
        }

        introImage.preserveAspect = true;

        if (introAspectFitter == null || introImage.sprite == null)
        {
            return;
        }

        Rect spriteRect = introImage.sprite.rect;
        if (spriteRect.height <= 0f)
        {
            return;
        }

        introAspectFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        introAspectFitter.aspectRatio = spriteRect.width / spriteRect.height;
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
