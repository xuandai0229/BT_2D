using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rb;
    public Rigidbody2D Rb => _rb;

    private Animator _ani;
    public Animator Ani => _ani;
    private Vector2 _moveInput;
    private float _jumpInput;

    public PlayerIdleSteta IdleSteta;
    public PlayerRunSteta RunSteta;
    public PlayerJumpSteta JumpSteta;

    private StateMachine _stateMachine;
    public StateMachine StateMachine => _stateMachine;

    public bool IsJumping;
    public bool IsRunning;

    public float JumpForce = 5f;

    private void Start()
    {
        IdleSteta = new PlayerIdleSteta(this);
        RunSteta = new PlayerRunSteta(this);
        JumpSteta = new PlayerJumpSteta(this);
        //_stateMachine.ChangeState = IdleSteta;
    }

    private void Update()
    {
        _stateMachine.CurrentState.Update();
    }

    public void Move(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        _jumpInput =  ctx.ReadValue<float>();
    }
}
