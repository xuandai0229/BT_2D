using UnityEngine;

public class Pipe : MonoBehaviour
{
    PipeController _controller;
    [SerializeField] private float _moveSpeed;
    [SerializeField] float _lifeTime;
    bool _isRelease;
    float _spawnTime;
    void Update()
    {
        if (Time.time > (_spawnTime + _lifeTime))
        {
            Deactivate();
        }
        transform.position += new Vector3(_moveSpeed * Time.deltaTime * -1f, 0f, 0f);
    }
    public void SetController(PipeController controller)
    {
        _controller = controller;
    }
    public void SpawnAt(Vector3 position)
    {
        _isRelease = false;
        transform.position = position;
    }
    public void SetSpawnTime(float time)
    {
        _spawnTime = time;
    }
    public void Deactivate()
    {
        if (_isRelease)
            return;
        _isRelease = true;
        _controller.Release(this);
    }
}
