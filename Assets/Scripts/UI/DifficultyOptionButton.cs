using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DifficultyOptionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private DifficultySelectPopupController popupController;
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private int difficultyLevel = 1;
    [SerializeField] private float transitionDuration = 0.18f;

    private Image _transitionOverlay;
    private Tween _transitionTween;

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        EnsureTransitionOverlay();
        ResetVisualState();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TransitionTo(hoverSprite);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TransitionTo(normalSprite);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        popupController?.ConfirmDifficulty(difficultyLevel);
    }

    public void ResetVisualState()
    {
        _transitionTween?.Kill();

        if (targetImage == null || normalSprite == null)
            return;

        EnsureTransitionOverlay();
        targetImage.sprite = normalSprite;
        SetImageAlpha(targetImage, 1f);
        SetImageAlpha(_transitionOverlay, 0f);
    }

    private void TransitionTo(Sprite targetSprite)
    {
        if (targetImage == null || targetSprite == null)
            return;

        EnsureTransitionOverlay();
        _transitionTween?.Kill();

        if (targetImage.sprite == targetSprite)
        {
            SetImageAlpha(targetImage, 1f);
            SetImageAlpha(_transitionOverlay, 0f);
            return;
        }

        SyncOverlayVisuals();
        _transitionOverlay.sprite = targetImage.sprite;
        SetImageAlpha(_transitionOverlay, 1f);

        targetImage.sprite = targetSprite;
        SetImageAlpha(targetImage, 0f);

        Sequence sequence = DOTween.Sequence();
        sequence.Join(_transitionOverlay.DOFade(0f, transitionDuration));
        sequence.Join(targetImage.DOFade(1f, transitionDuration));
        sequence.SetUpdate(true);
        _transitionTween = sequence;
    }

    private void EnsureTransitionOverlay()
    {
        if (_transitionOverlay != null || targetImage == null)
            return;

        var overlayObject = new GameObject("TransitionOverlay");
        overlayObject.transform.SetParent(targetImage.transform, false);
        overlayObject.transform.SetAsFirstSibling();

        var rectTransform = overlayObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;

        _transitionOverlay = overlayObject.AddComponent<Image>();
        _transitionOverlay.raycastTarget = false;
        SyncOverlayVisuals();
        SetImageAlpha(_transitionOverlay, 0f);
    }

    private void SyncOverlayVisuals()
    {
        if (_transitionOverlay == null || targetImage == null)
            return;

        _transitionOverlay.material = targetImage.material;
        _transitionOverlay.type = targetImage.type;
        _transitionOverlay.preserveAspect = targetImage.preserveAspect;
        _transitionOverlay.fillCenter = targetImage.fillCenter;
        _transitionOverlay.fillMethod = targetImage.fillMethod;
        _transitionOverlay.fillAmount = targetImage.fillAmount;
        _transitionOverlay.fillClockwise = targetImage.fillClockwise;
        _transitionOverlay.fillOrigin = targetImage.fillOrigin;
        _transitionOverlay.useSpriteMesh = targetImage.useSpriteMesh;
        _transitionOverlay.pixelsPerUnitMultiplier = targetImage.pixelsPerUnitMultiplier;
    }

    private static void SetImageAlpha(Graphic image, float alpha)
    {
        if (image == null)
            return;

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    private void OnDisable()
    {
        _transitionTween?.Kill();
        SetImageAlpha(_transitionOverlay, 0f);
        SetImageAlpha(targetImage, 1f);
    }
}
