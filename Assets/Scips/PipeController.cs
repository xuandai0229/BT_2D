using System.Collections.Generic;
using UnityEngine;

public class PipeController : MonoBehaviour
{
    Queue<Pipe> _pool = new();
    [SerializeField] Transform _spawnPosition;
    [SerializeField] Pipe _pipePrefab;
    [SerializeField] int _initialCapacity = 5;
    [SerializeField] int _maxSize = 10;
    [SerializeField] float _spawnRange;
    [SerializeField] float _spawnInterval;
    List<Pipe> _spawnedPipe = new();
    void Start()
    {
        for (var i = 0; i < _initialCapacity; i++)
        {
            var pipe = Create();
            pipe.gameObject.SetActive(false);
            _pool.Enqueue(pipe);
        }
        InvokeRepeating(nameof(SpawnPipe), 0f, _spawnInterval);
    }
    void SpawnPipe()
    {
        var pipe = Get();
        _spawnedPipe.Add(pipe);
    }
    Pipe Create()
    {
        var spawnPoint = CreateRandomSpawnPoint();
        var pipe = Instantiate(_pipePrefab, spawnPoint, _spawnPosition.rotation);
        pipe.SetController(this);
        pipe.SetSpawnTime(Time.time);
        return pipe;
    }
    Pipe Get()
    {
        Pipe pipe = _pool.Count > 0 ? _pool.Dequeue() : Create();
        pipe.SpawnAt(CreateRandomSpawnPoint());
        pipe.SetSpawnTime(Time.time);
        pipe.gameObject.SetActive(true);
        return pipe;
    }
    public void Release(Pipe pipe)
    {
        _spawnedPipe.Remove(pipe);
        pipe.gameObject.SetActive(false);
        if (_pool.Count >= _maxSize)
        {
            Destroy(pipe.gameObject);
            return;
        }
        _pool.Enqueue(pipe);
    }
    Vector2 CreateRandomSpawnPoint()
    {
        var verticalPos = Random.Range(_spawnPosition.position.y - _spawnRange, _spawnPosition.position.y + _spawnRange);
        return new Vector2(_spawnPosition.position.x, verticalPos);
    }
}
