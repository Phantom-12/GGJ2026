using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class DifficultySelectPopupController : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private bool hideOnAwake = true;
    [SerializeField] private DifficultyOptionButton[] optionButtons;

    private CanvasGroup _canvasGroup;
    private Tween _fadeTween;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        ResetOptionVisuals();

        if (hideOnAwake)
            HideImmediate();
        else
            ShowImmediate();
    }

    public void OpenPopup()
    {
        ResetOptionVisuals();
        FadeTo(1f, true);
    }

    public void ClosePopup()
    {
        FadeTo(0f, false);
    }

    public void ConfirmDifficulty(int level)
    {
        GameManager.Instance.SetDifficultyLevel(level);
        ClosePopup();
        AnimationManager.Instance.StartGameAnimation();
        GameManager.Instance.StartGame();
        AudioManager.Instance.PlayButtonClickSfx();
    }

    private void FadeTo(float targetAlpha, bool visible)
    {
        _fadeTween?.Kill();
        SetInteractable(visible);

        _fadeTween = _canvasGroup
            .DOFade(targetAlpha, fadeDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                if (!visible)
                    SetInteractable(false);
            });
    }

    private void HideImmediate()
    {
        _fadeTween?.Kill();
        _canvasGroup.alpha = 0f;
        SetInteractable(false);
    }

    private void ShowImmediate()
    {
        _fadeTween?.Kill();
        _canvasGroup.alpha = 1f;
        SetInteractable(true);
    }

    private void SetInteractable(bool visible)
    {
        _canvasGroup.interactable = visible;
        _canvasGroup.blocksRaycasts = visible;
    }

    private void ResetOptionVisuals()
    {
        foreach (var optionButton in optionButtons)
        {
            if (optionButton != null)
                optionButton.ResetVisualState();
        }
    }

    private void OnDestroy()
    {
        _fadeTween?.Kill();
    }
}
