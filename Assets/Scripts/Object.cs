using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class Object : MonoBehaviour
{
    enum State
    {
        BeforeAppear,
        Moving,
        PushedDown,
    }
    
    [Header("引用")] [SerializeField] private Transform reference;
    [SerializeField] private Transform target;
    [SerializeField] private ParticleSystem particle;

    [Header("配置")] [SerializeField] private float moveSpeed = 1;
    [SerializeField] private float timeFactor = 0;
    [SerializeField] private float maxBackSpeed = 1;
    [SerializeField] private float inertiaFactor = 1;

    private Vector2 _curAccVelocity = Vector2.zero;
    private float _lastCenterTime = 0;
    private State _state = State.BeforeAppear;

    private void Start()
    {
        target.localScale = new Vector2(0, 0);
        _lastCenterTime = Time.time;
        Appear();
    }

    private void Update()
    {
        if (_state == State.Moving)
        {
            UpdateVelocity();
            Move();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PushDown();
            }
        }
    }

    // 出现效果
    public void Appear()
    {
        _state = State.Moving;
        target.localScale = Vector3.zero;
        target.position = Vector3.zero;
        particle.Play();
        target.DOScale(Vector2.one, 0.2f).SetEase(Ease.OutBack);
    }

    // 放下效果
    public void PushDown()
    {
        Debug.Log(CalcOverlapArea());
        target.localScale = new Vector2(0, 0);
        _state = State.PushedDown;
    }

    // 计算重合面积
    public float CalcOverlapArea()
    {
        return Vector2.Distance(target.position, reference.position);
    }

    private void UpdateVelocity()
    {
        float deltaTime = Time.time - _lastCenterTime;
        var randomValueX = Random.value * (Random.value < 0.5 ? 1 : -1);
        _curAccVelocity.x += randomValueX;
        var randomValueY = Random.value * (Random.value < 0.5 ? 1 : -1);
        _curAccVelocity.y += randomValueY;
        var backVelocity = deltaTime * timeFactor * (Vector2)(reference.position - target.position).normalized;
        if (backVelocity.magnitude > maxBackSpeed)
        {
            backVelocity = backVelocity.normalized * maxBackSpeed;
        }
        _curAccVelocity += backVelocity;
        _curAccVelocity.Normalize();
        _curAccVelocity *= inertiaFactor;
    }

    private void Move()
    {
        target.position += (Vector3)(_curAccVelocity.normalized * (moveSpeed * Time.deltaTime));
        float deltaTime = Time.time - _lastCenterTime;
        if (Vector2.Distance(reference.position, target.position) <= 0.1f && deltaTime > 0.5)
        {
            _lastCenterTime = Time.time;
        }
    }
}