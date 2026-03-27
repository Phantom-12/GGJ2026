using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class OperableObject : MonoBehaviour
{
    private static readonly int Time1 = Shader.PropertyToID("_Time_");
    private static readonly int Speed = Shader.PropertyToID("_Speed");

    enum State
    {
        BeforeAppear,
        Moving,
        PutDown,
    }

    [Header("引用")][SerializeField] private Transform reference;
    [SerializeField] private Transform targetProxy;
    [SerializeField] private Transform target;
    [SerializeField] private Transform targetShower;
    [SerializeField] private SpriteMask spriteMask;
    [SerializeField] private Transform handLeft;
    [SerializeField] private Transform handRight;
    [SerializeField] private Transform putDownShower;
    [SerializeField] private ParticleSystem particleAppear;
    [SerializeField] private ParticleSystem particlePutDown;
    [SerializeField] private ParticleSystem particlePutDownPerfect;

    [Header("配置")][SerializeField] private Distance2Score scoreCfg;
    [Min(0f)][SerializeField] private float minRadius = 0f;
    [Min(0f)][SerializeField] private float maxRadius = 1f;
    [SerializeField] private float putDownShowDuration = 2f;
    [SerializeField] private float limitAngle = 60;
    private PutDownAutoAlignConfig _putDownAutoAlignConfig;

    private float _moveSpeedDefault, moveSpeed = 1;
    private State _state = State.BeforeAppear;
    private Tween _moveTween;
    private SpriteRenderer _spriteRenderer;
    private SpriteRenderer _putDownShowSpriteRenderer;

    private void Start()
    {
        _spriteRenderer = targetShower.GetComponent<SpriteRenderer>();
        _putDownShowSpriteRenderer = putDownShower.GetComponent<SpriteRenderer>();
        _moveSpeedDefault = moveSpeed;
        targetProxy.localScale = new Vector2(0, 0);
        putDownShower.gameObject.SetActive(false);
        gameObject.SetActive(false);
        transform.position = AnimationManager.Instance.originalPosition;
    }

    private void Update()
    {
    }

    public void SetDefaultMoveSpeed(float speed)
    {
        _moveSpeedDefault = speed;
    }

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    public void ResetMoveSpeed()
    {
        moveSpeed = _moveSpeedDefault;
    }

    public void SetPutDownAutoAlignConfig(PutDownAutoAlignConfig config)
    {
        _putDownAutoAlignConfig = config;
    }

    // 出现效果
    public void Appear()
    {
        if (_state != State.BeforeAppear)
        {
            return;
        }

        targetShower.SetParent(target, true);
        targetShower.localScale = Vector3.one;

        _spriteRenderer.sprite = ItemManager.Instance.GetItemMembrane();
        spriteMask.sprite = ItemManager.Instance.GetItemMembrane();
        _putDownShowSpriteRenderer.sprite = ItemManager.Instance.GetItemMembrane();

        gameObject.SetActive(true);
        _state = State.Moving;
        targetProxy.localScale = Vector3.zero;
        targetProxy.position = RandomCirclePoint(reference.position, minRadius, maxRadius);
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
        TryAutoAlignOnPutDown();

        // 粒子
        particlePutDown.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particlePutDownPerfect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        switch (level)
        {
            case 1:
            case 2:
            case 3:
                particlePutDown.Play();
                break;
            case 4:
                particlePutDownPerfect.Play();
                break;
        }

        // 音效
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
            case 4:
                AudioManager.Instance.PlaySfx(AudioType.BestAlignmentSfx);
                break;
        }

        await ShowPutDown();
        targetShower.SetParent(ItemManager.Instance.GetCurrentItemObject().transform, true);
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

        for (int i = scoreCfg.data.Count - 1; i >= 0; i--)
        {
            if (dis < scoreCfg.data[i].distance)
            {
                return (i + 1, scoreCfg.data[i].score);
            }
        }

        return (1, scoreCfg.data[0].score);
    }

    private void TryAutoAlignOnPutDown()
    {
        if (_putDownAutoAlignConfig == null)
        {
            return;
        }

        if (CalcDistance() > _putDownAutoAlignConfig.autoAlignDistance)
        {
            return;
        }

        targetProxy.position = reference.position;
    }

    private Vector2 RandomCirclePoint(Vector2 center, float minRadius, float maxRadius)
    {
        minRadius = Mathf.Max(0f, minRadius);
        maxRadius = Mathf.Max(minRadius, maxRadius);

        var direction = Random.insideUnitCircle;
        if (direction == Vector2.zero)
        {
            direction = Vector2.right;
        }
        direction.Normalize();

        var distance = Mathf.Sqrt(Random.Range(minRadius * minRadius, maxRadius * maxRadius));
        return center + direction * distance;
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

        // 计算随机位置，大于指定角度则重新选取
        var randomPos = RandomCirclePoint(reference.position, minRadius, maxRadius);
        for (var i = 0; i < 10000; i++)
        {
            var curPos = (Vector2)targetProxy.position;
            var refPos = (Vector2)reference.position;
            var refVec = refPos - curPos;
            var tarVec = randomPos - refPos;
            var angle = Vector2.Angle(refVec, tarVec);
            if (angle < limitAngle)
            {
                break;
            }

            randomPos = RandomCirclePoint(reference.position, minRadius, maxRadius);
        }

        Vector3[] points = { reference.position, randomPos };
        await targetProxy.DOPath(points, moveSpeed, PathType.CatmullRom)
            .SetSpeedBased(true)
            .SetEase(curve)
            .SetId("Move")
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
