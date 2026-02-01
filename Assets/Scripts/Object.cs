using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Object : MonoBehaviour
{
    private static readonly int Time1 = Shader.PropertyToID("_Time_");
    private static readonly int Speed = Shader.PropertyToID("_Speed");

    enum State
    {
        BeforeAppear,
        Moving,
        PutDown,
    }

    [Header("引用")] [SerializeField] private Transform reference;
    [SerializeField] private Transform targetProxy;
    [SerializeField] private Transform target;
    [SerializeField] private Transform targetShower;
    [SerializeField] private SpriteMask spriteMask;
    [SerializeField] private Transform handLeft;
    [SerializeField] private Transform handRight;
    [SerializeField] private Transform putDownShower;
    [SerializeField] private ParticleSystem particleAppear;
    [SerializeField] private ParticleSystem particlePutDown;

    [Header("配置")] [SerializeField] private Distance2Score scoreCfg;
    [SerializeField] private float moveDuration = 1;
    [SerializeField] private float radius = 2f;
    [SerializeField] private float putDownShowDuration = 2f;

    private State _state = State.BeforeAppear;
    private Tween _moveTween;
    private SpriteRenderer _spriteRenderer;
    private SpriteRenderer _putDownShowSpriteRenderer;

    private void Start()
    {
        _spriteRenderer = targetShower.GetComponent<SpriteRenderer>();
        _putDownShowSpriteRenderer = putDownShower.GetComponent<SpriteRenderer>();
        targetProxy.localScale = new Vector2(0, 0);
        putDownShower.gameObject.SetActive(false);
        gameObject.SetActive(false);
        transform.position = AnimationManager.Instance.originalPosition;
    }

    private void Update()
    {
    }

    // 出现效果
    public void Appear()
    {
        if (_state != State.BeforeAppear)
        {
            return;
        }
        targetShower.SetParent(target,true);
        targetShower.localScale = Vector3.one;

        _spriteRenderer.sprite = ItemManager.Instance.GetItemMembrane();
        spriteMask.sprite = ItemManager.Instance.GetItemMembrane();
        _putDownShowSpriteRenderer.sprite = ItemManager.Instance.GetItemMembrane();

        gameObject.SetActive(true);
        _state = State.Moving;
        targetProxy.localScale = Vector3.zero;
        targetProxy.position = RandomCirclePoint(reference.position, radius);
        particleAppear.Play();
        targetProxy.DOScale(Vector2.one, 0.2f).SetEase(Ease.OutBack);
        // target.DOShakePosition(10f, 0.1f, 10, 90, false, false).SetLoops(-1);
        StartMove().Forget();
    }

    // 放下效果
    public async UniTask<(int, int)> PutDown()
    {
        if (_state != State.Moving)
        {
            return (0, 0);
        }

        var (level, score) = CalcScore();

        _state = State.PutDown;
        StopMove();
        particlePutDown.Play();
        switch (level)
        {
            case 1:
                AudioManager.Instance.PlaySfx(AudioType.FailAlignmentSfx);
                break;
            case 2:
                AudioManager.Instance.PlaySfx(AudioType.NormalAlignmentSfx);
                break;
            case 3:
                AudioManager.Instance.PlaySfx(AudioType.PerfectAlignmentSfx);
                break;
        }

        await ShowPutDown();
        targetShower.SetParent(ItemManager.Instance.GetCurrentItemObject().transform,true);
        targetProxy.DOScale(Vector2.zero, 0.2f).SetEase(Ease.OutBack);
        _state = State.BeforeAppear;
        return (level, score);
    }

    public void Hide()
    {
        _state = State.BeforeAppear;
        targetProxy.localScale = Vector3.zero;
        StopMove();
    }

    // 计算距离
    private float CalcDistance()
    {
        return Vector2.Distance(targetProxy.position, reference.position);
    }

    public (int, int) CalcScore()
    {
        var dis = CalcDistance();
        for (int i = 0; i < scoreCfg.data.Count; i++)
        {
            if (dis < scoreCfg.data[i].distance)
            {
                return (3 - i, scoreCfg.data[i].score);
            }
        }

        return (0, 0);
    }

    private Vector2 RandomCirclePoint(Vector2 center, float radius)
    {
        return Random.insideUnitCircle * radius + center;
    }

    private async UniTaskVoid StartMove()
    {
        while (_state == State.Moving)
        {
            await MoveOnce();
        }
    }

    private void StopMove()
    {
        DOTween.Kill("Move");
    }

    private async UniTask MoveOnce()
    {
        var keyNums = 60;
        float sum = 0;
        float[] sums = new float[keyNums];
        Keyframe[] keyframes = new Keyframe[keyNums];
        for (int i = 0; i < keyNums; i++)
        {
            var t = (float)i / keyNums;
            if (t < 0.2)
            {
                sum += 5 * t;
            }
            else if (t > 0.8)
            {
                sum += -5 * t + 5;
            }
            else
            {
                sum += 50f / 9 * (t - 0.5f) * (t - 0.5f) + 0.5f;
            }

            sums[i] = sum;
        }

        for (int i = 0; i < keyNums; i++)
        {
            var t = (float)i / keyNums;
            keyframes[i] = new Keyframe(t, sums[i] / sum);
        }

        AnimationCurve curve = new AnimationCurve(keyframes);
        Vector3[] points = { reference.position, RandomCirclePoint(reference.position, radius) };
        await targetProxy.DOPath(points, moveDuration, PathType.CatmullRom).SetEase(curve).SetId("Move")
            .AsyncWaitForCompletion();
    }

    private async UniTask ShowPutDown()
    {
        putDownShower.gameObject.SetActive(true);
        var renderer = putDownShower.GetComponent<Renderer>();
        float time = putDownShowDuration;
        renderer.material.SetFloat(Speed, 1f / putDownShowDuration);
        while (true)
        {
            time -= Time.deltaTime;
            if (time <= 0)
                break;
            renderer.material.SetFloat(Time1, time);
            await UniTask.Yield();
        }

        putDownShower.gameObject.SetActive(false);
    }
}