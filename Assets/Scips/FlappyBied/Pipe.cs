using UnityEngine;
using UnityEngine.Pool;

public class Pipe : MonoBehaviour
{
    IObjectPool<Pipe> _pool;
    [SerializeField] private float _moveSpeed = 2f;
    bool _isReleased;
    public IObjectPool<Pipe> Pool
    {
        get => _pool;
        set => _pool = value;
    }
    [SerializeField] private float _pipeLifeTime = 5f;
    private float _spawnTime;

    void Update()
    {
        if (Time.time > (_spawnTime + _pipeLifeTime))
        {
            Deactivate();
        }
        transform.position += new Vector3(_moveSpeed * Time.deltaTime * -1f, 0f, 0f);
    }
    public void SpawnAt(Vector2 position)
    {
        _isReleased = false;
        transform.position = position;
    }
    public void SetSpawnTime(float time)
    {
        _spawnTime = time;
    }
    public void Deactivate()
    {
        if (_isReleased)
            return;
        _isReleased = true;
        _pool.Release(this);
    }
}