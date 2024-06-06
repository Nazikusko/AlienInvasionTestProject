using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EnemyCharacter : MonoBehaviour
{
    private const float DED_ENEMY_FLIGHT_HEIGHT = 1.5f;
    private const float DED_ENEMY_FLIGHT_TIME = 0.7f;
    private const float ATTACK_TIMOUT = 0.5f;
    private const string ANIMATOR_MOVE_TRIGGER_NAME = "IsMoving";
    private const float HEALSE_BAR_OFFSET_POINT = 2.3f;

    public event Action<EnemyCharacter> OnKilled;
    public event Action<EnemyCharacter> OnFlyComplete;

    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Collider _collider;
    [SerializeField] private Animator _animator;

    private Slider _healthBar;
    private RectTransform _healthBarRectTransform;
    private Camera _mainCamera;
    private RectTransform _canvasRectTransform;
    private Canvas _canvas;
    private ComponentPoolFactory _healseBarPoolFactory;
    private float _currentHealth;
    private float _fullHealth;
    private float _lastAttackTime;
    private bool _isMove;
    private Vector3 _lastAttackVector;

    public bool IsKilled => _currentHealth <= 0;
    public bool IsInAttack => _lastAttackTime > 0 &&
                              _lastAttackTime + ATTACK_TIMOUT > Time.time;


    private void Awake()
    {
    }

    private void Update()
    {
        UpdateHealthBarPosition();

        if (!_isMove && IsInAttack)
        {
            _isMove = true;
            transform.DOMove(transform.position + _lastAttackVector * 3f, 4f).SetEase(Ease.InOutSine).SetId(this)
                .OnComplete(OnMoveComplete);
            transform.DORotateQuaternion(Quaternion.LookRotation(_lastAttackVector), 0.3f).SetEase(Ease.InOutSine).SetId(this);
            _animator.SetBool(ANIMATOR_MOVE_TRIGGER_NAME, true);
        }
    }

    private void OnDisable()
    {
        DOTween.Kill(this);
        DOTween.Kill(gameObject);
    }

    public void Initialize(float health, Canvas uiCanvas)
    {
        _currentHealth = health;
        _fullHealth = health;
        _canvas = uiCanvas;
        _collider.enabled = true;
        _isMove = false;
        _animator.SetBool(ANIMATOR_MOVE_TRIGGER_NAME, false);
        _lastAttackTime = -1;
        transform.eulerAngles = new Vector3(0, Random.Range(0, 360f), 0);

        if (_canvasRectTransform == null)
            _canvasRectTransform = _canvas.GetComponent<RectTransform>();

        if (_mainCamera == null)
            _mainCamera = uiCanvas.worldCamera;

        if (_healseBarPoolFactory == null)
            _healseBarPoolFactory = _canvas.GetComponentInChildren<ComponentPoolFactory>();

        _healthBar = _healseBarPoolFactory.Get<Slider>();
        _healthBarRectTransform = _healthBar.GetComponent<RectTransform>();
        UpdateHealthBar();
    }

    public void Dispose()
    {
        _healseBarPoolFactory.Release(_healthBar);
    }

    private void OnCollisionEnter(Collision collision)
    {
        DOTween.Kill(this);
        _animator.SetBool(ANIMATOR_MOVE_TRIGGER_NAME, false);
    }

    private void OnMoveComplete()
    {
        _isMove = false;
        _animator.SetBool(ANIMATOR_MOVE_TRIGGER_NAME, false);
    }

    public bool Attack(float force, Vector3 attackerPosition)
    {
        _currentHealth -= Config.Instance.AttackUpdateTime * force;
        UpdateHealthBar();
        if (_currentHealth <= 0)
        {
            OnKilled?.Invoke(this);
            return true;
        }
        _lastAttackVector = (transform.position - new Vector3(attackerPosition.x, 0, attackerPosition.z)).normalized;
        _lastAttackTime = Time.time;
        return false;
    }

    private void UpdateHealthBar()
    {
        _healthBar.value = Mathf.Max(_currentHealth, 0) / _fullHealth;
    }

    private void UpdateHealthBarPosition()
    {
        Vector3 screenPoint = _mainCamera.WorldToScreenPoint(transform.position + Vector3.up * HEALSE_BAR_OFFSET_POINT);

        if (screenPoint.x > 0 && screenPoint.x < Screen.width && screenPoint.y > 0 && screenPoint.y < Screen.height && !IsKilled)
        {
            if (!_healthBar.gameObject.activeSelf)
            {
                _healthBar.gameObject.SetActive(true);
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRectTransform, screenPoint, _mainCamera,
                out var canvasPosition);
            _healthBarRectTransform.anchoredPosition = canvasPosition;
        }
        else
        {
            _healthBar.gameObject.SetActive(false);
        }
    }

    public void StartFlyToPlayer(Transform playerTransform)
    {
        DOTween.Kill(this);
        transform.parent = playerTransform;
        _collider.enabled = false;
        var path = new Vector3[2];
        path[0] = Vector3.Lerp(Vector3.zero, transform.localPosition, 0.5f);
        path[0].y += DED_ENEMY_FLIGHT_HEIGHT;
        path[1] = Vector3.zero;

        transform.DOLocalPath(path, DED_ENEMY_FLIGHT_TIME, pathType: PathType.CatmullRom)
            .OnComplete(FlyComplete).SetEase(Ease.InQuad).SetId(transform);
    }

    private void FlyComplete()
    {
        OnFlyComplete?.Invoke(this);
    }
}
