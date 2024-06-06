using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : Singleton<EnemySpawner>
{
    [SerializeField] private ComponentPoolFactory _poolFactory;
    [SerializeField] private Canvas _canvas;

    private Vector2[] _minMaxSpawnPositionPoints;
    private List<EnemyCharacter> _enemyCharacterList;
    private float _lastSpawnTime;

    protected override void DoAwake()
    {
        _enemyCharacterList = new List<EnemyCharacter>(Config.Instance.MaxEnemySpawnCount);
        GetMinMaxSpawnPositions();

        for (int i = 0; i < Config.Instance.MaxEnemySpawnCount; i++)
        {
            SpawnEnemy();
        }
    }

    private void Update()
    {
        if (_enemyCharacterList.Count < Config.Instance.MaxEnemySpawnCount 
            && _lastSpawnTime + Config.Instance.EnemySpawnInterval <= Time.time)
        {
            SpawnEnemy();
        }
    }

    protected override void DoDestroy()
    {
        foreach (var character in _enemyCharacterList)
        {
            character.OnKilled -= OnKilled;
            character.OnFlyComplete -= OnFlyComplete;
        }
    }

    private void SpawnEnemy()
    {
        if (_enemyCharacterList.Count >= Config.Instance.MaxEnemySpawnCount)
            return;

        var newEnemy = _poolFactory.Get<EnemyCharacter>();
        var x = Random.Range(_minMaxSpawnPositionPoints[0].x, _minMaxSpawnPositionPoints[1].x);
        var z = Random.Range(_minMaxSpawnPositionPoints[0].y, _minMaxSpawnPositionPoints[1].y);

        newEnemy.transform.position = new Vector3(x, 0, z);

        newEnemy.Initialize(Config.Instance.EenemyDefaultHp, _canvas);
        newEnemy.OnKilled += OnKilled;
        newEnemy.OnFlyComplete += OnFlyComplete;
        _enemyCharacterList.Add(newEnemy);
        _lastSpawnTime = Time.time;
    }

    private void OnKilled(EnemyCharacter enemy)
    {
        var killedEnemy = _enemyCharacterList.Find(e => e == enemy);
        if (killedEnemy == null) return;

        killedEnemy.OnKilled -= OnKilled;
        killedEnemy.Dispose();
    }

    private void OnFlyComplete(EnemyCharacter enemy)
    {
        var eatenEnemy = _enemyCharacterList.Find(e => e == enemy);
        if (eatenEnemy == null) return;

        eatenEnemy.OnKilled -= OnFlyComplete;
        _poolFactory.Release(eatenEnemy);
        _enemyCharacterList.Remove(eatenEnemy);
    }

    private void GetMinMaxSpawnPositions()
    {
        var renderer = GetComponent<MeshRenderer>();
        var size = renderer.bounds.size;
        _minMaxSpawnPositionPoints = new Vector2[2];
        _minMaxSpawnPositionPoints[0].x = transform.position.x - (size.x / 2);
        _minMaxSpawnPositionPoints[0].y = transform.position.z - (size.z / 2);

        _minMaxSpawnPositionPoints[1].x = transform.position.x + (size.x / 2);
        _minMaxSpawnPositionPoints[1].y = transform.position.z + (size.z / 2);
    }
}
