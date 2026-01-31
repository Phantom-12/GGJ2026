using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Indicator : MonoBehaviour
{
    [SerializeField] private Object operableObject;
    [SerializeField] private Color[] scoreColors;

    private Image _image;
    private int _state = 0;
    private bool _show = false;

    private void Start()
    {
        _image = GetComponent<Image>();
        _image.transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        if(!_show)
            return;
        var (level, _) = operableObject.CalcScore();
        if (level != _state)
        {
            _image.color = scoreColors[level - 1];
            _state = level;
            ChangeState(level).Forget();
        }
    }

    public void Show()
    {
        _show = true;
        transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBounce);
    }

    public void Hide()
    {
        _show = false;
        transform.DOScale(Vector3.zero, 0.1f);
    }

    private async UniTaskVoid ChangeState(int state)
    {
        await transform.DOScale(Vector3.one * 1.2f, 0.1f).SetEase(Ease.InOutBounce).AsyncWaitForCompletion();
        await transform.DOScale(Vector3.one, 0.05f).SetEase(Ease.InOutBounce).AsyncWaitForCompletion();
    }
}