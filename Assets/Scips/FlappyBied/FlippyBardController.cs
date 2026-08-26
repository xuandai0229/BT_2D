using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FlippyBardController : MonoBehaviour
{
    [SerializeField] float _jumpForce = 5f;
    [SerializeField] LayerMask _collisionMask;
    private int _score;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    public Action Die;
    public Action<int> Score;
    Rigidbody2D _rb;
    bool _isJump;
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.Space)
            || Input.GetMouseButtonDown(0))
        {
            _isJump = true;
           // AudioManager.Instance.PlayWing();
        }
    }
    void FixedUpdate()
    {
        if (_isJump)
        {
            _rb.linearVelocityY = _jumpForce;
            _isJump = false;
        }
    }
    public void Reset()
    {
        transform.SetPositionAndRotation(_initialPosition, _initialRotation);
        _score = 0;
    }
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("ScoreZone"))
        {
            _score++;
            Score?.Invoke(_score);
            //AudioManager.Instance.PlayPoint();
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & _collisionMask.value) != 0)
        {
           // AudioManager.Instance.PlayHit();
            Die?.Invoke();
        }
    }
}
