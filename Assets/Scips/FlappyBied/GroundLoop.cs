using UnityEngine;

public class GroundLoop : MonoBehaviour
{
    [SerializeField] private Transform _firstGround;
    [SerializeField] private Transform _secondGround;
    [SerializeField] private float _scrollSpeed = 2f;
    private Camera _camera;
    private float _groundWidth;
    void Start()
    {
        _camera = Camera.main;
        _groundWidth = _firstGround.GetComponent<SpriteRenderer>().bounds.size.x;
        SnapBehind(_secondGround, _firstGround);
    }

    void Update()
    {
        float step = _scrollSpeed * Time.deltaTime;
        _firstGround.position += Vector3.left * step;
        _secondGround.position += Vector3.left * step;

        if (IsOffScreen(_firstGround))
        {
            SnapBehind(_firstGround, _secondGround);
        }
        else if (IsOffScreen(_secondGround))
        {
            SnapBehind(_secondGround, _firstGround);
        }
    }

    private bool IsOffScreen(Transform ground)
    {
        var cameraLeftEdge = _camera.transform.position.x - _camera.orthographicSize * _camera.aspect;
        return ground.position.x + _groundWidth * 0.5f < cameraLeftEdge;
    }

    private void SnapBehind(Transform ground, Transform other)
    {
        var position = ground.position;
        position.x = other.position.x + _groundWidth;
        ground.position = position;
    }
}
