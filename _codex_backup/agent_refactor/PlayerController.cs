using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rb;
    public Rigidbody2D Rb => _rb;

    private Animator _ani;
    public Animator Ani => _ani;

    private PlayerStateMachine _stateMachine;
    public PlayerStateMachine StateMachine => _stateMachine;


    public PlayerIdleState IdleState { get; private set; }

    public PlayerRunState RunState { get; private set; }

    public PlayerJumpState JumpState { get; private set; }
    public PlayerFallState FallState { get; private set; }
    public PlayerAttackState AttackState { get; private set; }

    private Vector2 _moveInput;

    private bool _jumpPressed;
    private bool _attackPressed;

    private bool _animationEvent;

    public bool AnimationEvent
    {
        get => _animationEvent;
        set => _animationEvent = value;
    }

    public float MoveX => _moveInput.x;

    public bool HasMoveInput =>Mathf.Abs(_moveInput.x) > 0.01f;

    [SerializeField]private float moveSpeed = 5f;
    public float RunSpeed => moveSpeed;

    [SerializeField] private float jumpForce = 7f;

    [SerializeField] private Transform groundCheck;

    [SerializeField] private float groundCheckRadius = 0.15f;

    [SerializeField] private LayerMask groundLayer;



    private void Awake()
    {
      
        _rb = GetComponent<Rigidbody2D>();

        _ani = GetComponent<Animator>();

  
        if (_ani == null)
        {
            _ani = GetComponentInChildren<Animator>();
        }


  
        _stateMachine = new PlayerStateMachine();


        IdleState = new PlayerIdleState(this);

        RunState = new PlayerRunState(this);

        JumpState = new PlayerJumpState(this);

        FallState = new PlayerFallState(this);
        AttackState = new PlayerAttackState(this);
    }


    private void Start()
    {
        
        _stateMachine.ChangeState(IdleState);
    }


    private void Update()
    {
        _stateMachine.Update();
    }


    private void FixedUpdate()
    {
        _stateMachine.FixedUpdate();
    }

    public void Move(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }


    public void Jump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            _jumpPressed = true;
        }
    }

    public void Attack(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            _attackPressed = true;
        }
    }


    public bool ConsumeJumpPressed()
    {
        if (!_jumpPressed)
            return false;

        _jumpPressed = false;

        return true;
    }

    public bool ConsumeAttackPressed()
    {
        if (!_attackPressed)
            return false;

        _attackPressed = false;

        return true;
    }
    public void SetHorizontalVelocity(float direction)
    {
        _rb.linearVelocity = new Vector2(direction * moveSpeed,_rb.linearVelocity.y );
    }


    public void StopHorizontal()
    {
        _rb.linearVelocity = new Vector2(0f,_rb.linearVelocity.y);
    }


    public void ApplyJump()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x,jumpForce);
    }


  
    public bool IsGrounded()
    {
        if (groundCheck == null)
            return false;

        return Physics2D.OverlapCircle(groundCheck.position,groundCheckRadius,groundLayer) != null;
    }


    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.DrawWireSphere(groundCheck.position,groundCheckRadius);
    }

    public void SetFacingDirection(float direction)
    {
        if (Mathf.Abs(direction) < 0.01f)
            return;

        if ((direction > 0) != (transform.localScale.x > 0))
        {
            Vector3 scale = transform.localScale;

            scale.x *= -1f;

            transform.localScale = scale;
        }
    }
    public void TriggerAnimationEvent()
    {
        _animationEvent = true;
    }
}