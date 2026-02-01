using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Serialization;

public class Bubble : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private float animDuration = 1;
    [SerializeField] private float animStayDuration = 1;
    [SerializeField] private float animDistance = 1;

    private Image _image;
    private RectTransform _rect;
    private Vector2 _oriPos;

    private void Start()
    {
        _image = GetComponent<Image>();
        _rect = GetComponent<RectTransform>();
        _rect.localScale = Vector3.zero;
        _oriPos = _rect.anchoredPosition;
    }

    public async UniTaskVoid ShowBubbleAnimation(int level)
    {
        _rect.anchoredPosition = _oriPos;
        _image.sprite = sprites[level - 1];
        _rect.DOScale(Vector2.one, animDuration / 2).SetEase(Ease.InOutSine);
        await _rect.DOAnchorPosY(_rect.anchoredPosition.y + animDistance / 2, animDuration).SetEase(Ease.InOutSine)
            .AsyncWaitForCompletion();
        await UniTask.WaitForSeconds(animStayDuration);
        _rect.DOScale(Vector2.zero, animDuration / 2).SetEase(Ease.InOutSine);
        await _rect.DOAnchorPosY(_rect.anchoredPosition.y + animDistance / 2, animDuration).SetEase(Ease.InOutSine)
            .AsyncWaitForCompletion();
    }
}