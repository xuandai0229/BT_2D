using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PipeController : MonoBehaviour
{
    [SerializeField] private Pipe _pipePrefab;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private float _spawnInterval = 1f;
    [SerializeField] private float _spawnRange = 2f;
    IObjectPool<Pipe> _pool;
    List<Pipe> _spawnedPipe = new();
    void Start()
    {
        _pool = new ObjectPool<Pipe>(
            OnCreate,
            OnGetAction,
            OnReleaseAction,
            OnDestroyAction,
            collectionCheck: true,
            defaultCapacity: 5,
            maxSize: 10
        );
        InvokeRepeating(nameof(SpawnPipeRepeat), 0f, _spawnInterval);
    }

    private void SpawnPipeRepeat()
    {
        Pipe pipe = _pool.Get();
        _spawnedPipe.Add(pipe);
    }

    private Pipe OnCreate()
    {
        var spawnPos = CreateRandomSpawnPoint();
        Pipe pipe = Instantiate(_pipePrefab, spawnPos, _spawnPoint.rotation);
        pipe.Pool = _pool;
        pipe.SetSpawnTime(Time.time);
        return pipe;
    }

    private void OnGetAction(Pipe pipe)
    {
        var spawnPos = CreateRandomSpawnPoint();
        pipe.SpawnAt(spawnPos);
        pipe.SetSpawnTime(Time.time);
        pipe.gameObject.SetActive(true);
    }

    private void OnReleaseAction(Pipe pipe)
    {
        pipe.gameObject.SetActive(false);
    }

    private void OnDestroyAction(Pipe pipe)
    {
        Destroy(pipe.gameObject);
    }

    Vector2 CreateRandomSpawnPoint()
    {
        var verticalPos = Random.Range(_spawnPoint.position.y - _spawnRange,
                                        _spawnPoint.position.y + _spawnRange);
        return new Vector3(_spawnPoint.position.x, verticalPos);
    }

    public void Restart()
    {
        foreach (var pipe in _spawnedPipe)
        {
            pipe.Deactivate();
        }
        _spawnedPipe.Clear();
    }
}
