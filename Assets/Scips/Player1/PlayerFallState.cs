using UnityEngine;

public class PlayerFallState : PlayerStateBase
{
    public PlayerFallState(PlayerController controller) : base(controller)
    {
    }

    public override void Enter()
    {
        base.Enter();
        _ani.SetBool("IsJump", false);
        _ani.SetBool("IsFall", true);
    }

    public override void Update()
    {
        base.Update();
        if (_controller.HasMoveInput)
        {
            _controller.SetFacingDirection(_controller.MoveX);
        }

        if (_controller.IsGrounded() && _rb.linearVelocity.y <= 0f)
        {
          
            if (_controller.HasMoveInput)
            {
                _stateMachine.ChangeState(_controller.RunState);
            }
            else
            {
                
                _stateMachine.ChangeState(_controller.IdleState);
            }

            return;
        }

    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        _rb.linearVelocity = new Vector2(_controller .MoveX * _controller.RunSpeed, _rb.linearVelocity.y);
    }

    public override void Exit()
    {
        base.Exit();
        _ani.SetBool("IsFall", false);
    }
}

