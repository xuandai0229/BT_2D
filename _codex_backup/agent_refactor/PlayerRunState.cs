using UnityEngine;

public class PlayerRunState : PlayerStateBase
{
    public PlayerRunState(PlayerController controller)
        : base(controller)
    {
    }

    public override void Enter()
    {
        _ani.SetBool("IsRun", true);
    }

    public override void Update()
    {
        base.Update();


        if (_controller.IsGrounded() && _controller.ConsumeJumpPressed())
        {
            _stateMachine.ChangeState( _controller.JumpState );
            return;
        }

        if (_controller.ConsumeAttackPressed())
        {
            _stateMachine.ChangeState( _controller.AttackState );
            return;
        }

        if (!_controller.IsGrounded())
        {
            _stateMachine.ChangeState( _controller.FallState );

            return;
        }

        if (!_controller.HasMoveInput)
        {
            _stateMachine.ChangeState(_controller.IdleState);

            return;
        }
        _controller.SetFacingDirection( _controller.MoveX );
    }

    public override void FixedUpdate()
    {
        _rb.linearVelocity = new Vector2(_controller.MoveX * _controller.RunSpeed,_rb.linearVelocity.y);
    }

    public override void Exit()
    {
        _ani.SetBool("IsRun", false);
    }
}