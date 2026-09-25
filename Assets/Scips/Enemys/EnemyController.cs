using UnityEngine;

public class EnemyController : AgentController
{
    public EnemyIdleState IdleState { get; private set; }
    public EnemyWalkState WalkState { get; private set; }
    public EnemyRunState RunState { get; private set; }

    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float idleTime = 2f;

    public float IdleTime => idleTime;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance = 0.6f;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDistance = 0.3f;

    private int _facingDirection = 1;
    public int FacingDirection => _facingDirection;

    private static readonly int MoveStateHash = Animator.StringToHash("MoveState");

    protected override void Awake()
    {
        base.Awake();
        IdleState = new EnemyIdleState(this);

        WalkState = new EnemyWalkState(this);

        RunState = new EnemyRunState(this);
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

    public  void SetMoveState(int moveState)
    {
        _ani.SetInteger(MoveStateHash, moveState);
    }

    public void Walk()
    {
        _rb.linearVelocity = new Vector2( FacingDirection * walkSpeed, _rb.linearVelocity.y );
    }

    public void Run()
    {
        _rb.linearVelocity = new Vector2( FacingDirection * runSpeed,_rb.linearVelocity.y);
    }

    public void StopMoving()
    {
        _rb.linearVelocity = Vector2.zero;
    }

    public bool IsGroundDetect
    {
        get
        {
            if (groundCheck == null)
            {
                return false;
            }
            RaycastHit2D hit =Physics2D.Raycast( groundCheck.position,Vector2.down,groundCheckDistance, groundLayer );
            return hit.collider != null;
        }
    }

    public bool IsWallDetected
    {
        get
        {
            if (wallCheck == null)
            {
                return false;
            }
            RaycastHit2D hit =Physics2D.Raycast(wallCheck.position, Vector2.right * FacingDirection, wallCheckDistance, groundLayer );
            return hit.collider != null;
        }
    }

    public void FlipDirection()
    {
        _facingDirection *= -1;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
        }


        if (wallCheck != null)
        {
            int direction =Application.isPlaying? _facingDirection : 1;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.right * direction * wallCheckDistance);
        }
    }
}
