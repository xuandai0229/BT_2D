using UnityEngine;

public class PlayerJumpState : PlayerStateBase
{
    public PlayerJumpState(PlayerController controller)
        : base(controller)
    {
    }

    public override void Enter()
    {
        _ani.SetBool("IsRun", false);

        _ani.SetBool("IsFall", false);

        _ani.SetBool("IsJump", true);

        _controller.ApplyJump();
    }

    public override void Update()
    {
        base.Update();

        if (_controller.HasMoveInput)
        {
            _controller.SetFacingDirection(_controller.MoveX);
        }

        if (_rb.linearVelocity.y <= 0f)
        {
            _stateMachine.ChangeState( _controller.FallState);

            return;
        }
    }

    public override void FixedUpdate()
    {
        _rb.linearVelocity = new Vector2(_controller.MoveX * _controller.RunSpeed,_rb.linearVelocity.y);
    }

    public override void Exit()
    {
        _ani.SetBool("IsJump", false);
    }
}