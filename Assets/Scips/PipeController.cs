using UnityEditor.Search;
using UnityEngine;

public class PipeController : MonoBehaviour
{
    [SerializeField] Transform _spawPosition;

    Query<Pipe> _pool = new();

    int _initiaCapacity = 5;

    private void Start()
    {
        for (var i = 0; i < _initiaCapacity; i++)
        {
            var pipe = Instantiate(_pipePrefeb, _spawPosition);
        }
    }



    Pipe Get()
    {
        var pipe = _pool.Deque
    }
}
