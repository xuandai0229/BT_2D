
using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public enum AnimState
    {
        Idle = 0,
        Run = 1,
        Jump = 2,
        Fall = 3,
        Attack = 4
    }

    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _jumpForce = 5f;

    //[SerializeField] private Transform _groundCheck;
    //[SerializeField] private float _groundCheckRadius = 0.15f;
    //[SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask _groundLayer;

    private Rigidbody2D _rb;
    private Animator _ani;

    private AnimState _state = AnimState.Idle;

    private float _moveHorizontal;
    private bool _isGrounded;
     private bool _isJumping;
    private bool _isAttacking;
    

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _ani = GetComponent<Animator>();
    }

    private void Update()
    {
        _moveHorizontal = Input.GetAxisRaw("Horizontal");

        // _isGrounded = Physics2D.OverlapCircle(_groundCheck.position,_groundCheckRadius, _groundLayer);
        DetectGround();
        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            _isJumping = true;
            
        }
        //attack
        if (Input.GetKeyDown(KeyCode.J) && !_isAttacking)
        {
            _isAttacking = true;
            
            
        }
        //if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        //{
        //    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
        //}

        UpdateState();
        FlipPlayer();
        

    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = new Vector2(_moveHorizontal * _moveSpeed,_rb.linearVelocity.y);
        //jump
        if (_isJumping)
        {
            _rb.AddForce(_jumpForce * Vector2.up, ForceMode2D.Impulse);
            _isJumping = false;
        }
    }

    private void UpdateState()
    {
        if(_isAttacking)
        {
            _state = AnimState.Attack; 
        }
        else if (!_isGrounded && _rb.linearVelocity.y > 0.1f)
        {
            _state = AnimState.Jump;
        }
        else if (!_isGrounded && _rb.linearVelocity.y < -0.1f)
        {
            _state = AnimState.Fall;
        }
        else if (Mathf.Abs(_moveHorizontal) > 0.1f)
        {
            _state = AnimState.Run;
        }
        else
        {
            _state = AnimState.Idle;
        }
        _ani.SetInteger("State", (int)_state);
    }

    private void FlipPlayer()
    {
        if (_moveHorizontal < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (_moveHorizontal > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }


    private void DetectGround()
    {
        List<RaycastHit2D> hits = new List<RaycastHit2D>();

        ContactFilter2D contactFilter = new ContactFilter2D()
        {
            layerMask = _groundLayer, useLayerMask = true
        };

        if (Physics2D.Raycast( _groundCheck.position,  Vector2.down,contactFilter,hits,_groundCheckDistance) > 0)
        {
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }
    }

    public void EndAttack()
    {
        _isAttacking = false;
    }
    private void OnDrawGizmosSelected()
    {
        if (_groundCheck == null)
            return;

        Gizmos.color = Color.red;

        Gizmos.DrawRay(_groundCheck.position,Vector2.down * _groundCheckDistance);
    }
}