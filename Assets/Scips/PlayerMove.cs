using UnityEngine;
using System.Collections.Generic;

public class PlayerMove : MonoBehaviour
{
    public enum AminState
    {
        Idle = 0,
        Run = 1,
        Jump = 2,
        Fall = 3,
    }

    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _jumpForce = 5f;

    [SerializeField] private Transform _goroundCheck;
    [SerializeField] private float _groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask _groundLayer;



    private Rigidbody2D _rb;
    private Animator _ani;

    private AminState _state = AminState.Idle;

    private float _horizontalInput;
    private bool _isGrounded;
    private bool _isJumping;

    private void Awake()
    {
        _ani = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();


        _ani.SetInteger("State", (int)_state);

    }

    void Update()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            _isJumping = true;
        }

        Filip();
        UpdateState();

    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = new Vector2(_horizontalInput * _speed, _rb.linearVelocity.y);

        if (_isJumping)
        {
            _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
            _isJumping = false;
        }
    }

    private void UpdateState()
    {
        if (!_isGrounded && _rb.linearVelocity.y > 0.1f)
        {
            _state = AminState.Jump;
        }
        else if (!_isGrounded && _rb.linearVelocity.y < -0.1f)
        {
            _state = AminState.Fall;
        }
        else if (Mathf.Abs(_horizontalInput) > 0.1f)
        {
            _state = AminState.Run;
        }
        else
        {
            _state = AminState.Idle;
        }
        ChengeState(_state);
    }

    private void ChengeState(AminState newState)
    {
        if (_state == newState)
        {
            return;
        }

        _state = newState;
        _ani.SetInteger("State", (int)_state);
    }

    private void Filip()
    {
        if (_horizontalInput < 0f)
        {
            transform.localScale = new Vector2(-1f, 1f);
        }
        else if (_horizontalInput > 0f)
        {
            transform.localScale = new Vector2(1f, 1f);
        }
    }
}

