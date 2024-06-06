using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    private const string ANIMATOR_MOVE_TRIGGER_NAME = "IsMoving";
    private const int LINE_RENDERER_PINTS_COUNT = 16;

    [SerializeField] private Transform _radiusSprite;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private LineRenderer _lineRendererPrefab;
    [SerializeField] private Animator _animator;

    private float _attackRadius;
    private Vector3 _startRadiusSpriteLocalScale;
    private List<EnemyCharacter> _currentAttackedEnemys;
    private int _currentAttckedEnemysMaxCount;
    private int _attackedEnemyMaxCount;
    private LineRenderer[] _lineRenderers;


    private void Awake()
    {
        _attackedEnemyMaxCount = Config.Instance.AttackedEnemyMaxCount;
        _attackRadius = Config.Instance.PlayerDefaultAttackRadius;
        _currentAttckedEnemysMaxCount = _attackedEnemyMaxCount;
        _currentAttackedEnemys = new List<EnemyCharacter>(_currentAttckedEnemysMaxCount);

        _lineRenderers = new LineRenderer[_attackedEnemyMaxCount];
        for (int i = 0; i < _attackedEnemyMaxCount; i++)
        {
            _lineRenderers[i] = Instantiate(_lineRendererPrefab, transform);
            _lineRenderers[i].positionCount = LINE_RENDERER_PINTS_COUNT;
            _lineRenderers[i].gameObject.SetActive(false);
        }

        _startRadiusSpriteLocalScale = _radiusSprite.localScale;
        SetAttackRadius(_attackRadius);
    }

    private void Start()
    {
        StartCoroutine(UpdateAttack());
    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {
        var joystickState = JoystickInput.Instance.GetJoystickState();
        _rigidbody.velocity = new Vector3(joystickState.x, 0, joystickState.y) *
                                                        Config.Instance.PlayerDefaultMoveSpeed;
        _rigidbody.rotation = Quaternion.LookRotation(new Vector3(joystickState.x, 0, joystickState.y));

        if (joystickState.magnitude > 0.1f)
        {
            _animator.SetBool(ANIMATOR_MOVE_TRIGGER_NAME, true);
        }
        else
        {
            _animator.SetBool(ANIMATOR_MOVE_TRIGGER_NAME, false);
        }
    }

    private IEnumerator UpdateAttack()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(Config.Instance.AttackUpdateTime);
            UpdateEnemyAttackedList();
            AttackEnemyOnList();
        }
    }

    private void AttackEnemyOnList()
    {
        for (int i = 0; i < _currentAttackedEnemys.Count; i++)
        {
            if (!_currentAttackedEnemys[i].Attack(Config.Instance.PlayerDefaultAttackForce, transform.position))
            {
                _lineRenderers[i].gameObject.SetActive(true);

                for (int j = 0; j < LINE_RENDERER_PINTS_COUNT; j++)
                {
                    float progress = (float)j / LINE_RENDERER_PINTS_COUNT;
                    var point = Vector3.Lerp(transform.position, _currentAttackedEnemys[i].transform.position,
                        progress);

                    point.y = transform.position.y + Mathf.Sin(progress * Mathf.PI) * 2f;

                    _lineRenderers[i].SetPosition(j, point);
                }
            }
            else
            {
                _currentAttackedEnemys[i].StartFlyToPlayer(transform);

            }
        }

        if (_currentAttackedEnemys.Count < _attackedEnemyMaxCount)
        {
            for (int i = _currentAttackedEnemys.Count; i < _attackedEnemyMaxCount; i++)
            {
                _lineRenderers[i].gameObject.SetActive(false);
            }
        }
    }

    private void UpdateEnemyAttackedList()
    {
        var colliders = Physics.OverlapSphere(transform.position, _attackRadius, LayerMask.GetMask("Enemy"));
        var enemyCharactersInSphere = new List<EnemyCharacter>(colliders.Length);

        foreach (var collider in colliders)
        {
            enemyCharactersInSphere.Add(collider.gameObject.GetComponent<EnemyCharacter>());
        }

        if (enemyCharactersInSphere.Count == 0)
        {
            _currentAttackedEnemys.Clear();
        }
        else
        {
            _currentAttackedEnemys.RemoveAll(e => !enemyCharactersInSphere.Contains(e));
        }

        foreach (var enemyInSphere in enemyCharactersInSphere)
        {
            if (_currentAttackedEnemys.Count < _currentAttckedEnemysMaxCount &&
                !_currentAttackedEnemys.Contains(enemyInSphere))
            {
                _currentAttackedEnemys.Add(enemyInSphere);
            }
        }
    }

    private void SetAttackRadius(float radius)
    {
        _radiusSprite.localScale = _startRadiusSpriteLocalScale * radius * 2;
    }
}
