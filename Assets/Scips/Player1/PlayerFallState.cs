using UnityEngine;

public class PlayerFallState : AgentStateBase
{
    private readonly PlayerController _player;

    public PlayerFallState(PlayerController player)
        : base(player)
    {
        _player = player;
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

        if (_player.HasMoveInput)
        {
            _player.SetFacingDirection(_player.MoveX);
        }

        if (_player.IsGrounded() && _rb.linearVelocity.y <= 0f)
        {
            if (_player.HasMoveInput)
            {
                _stateMachine.ChangeState(_player.RunState);
            }
            else
            {
                _stateMachine.ChangeState(_player.IdleState);
            }

            return;
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        _rb.linearVelocity = new Vector2(_player.MoveX * _player.RunSpeed, _rb.linearVelocity.y);
    }

    public override void Exit()
    {
        base.Exit();
        _ani.SetBool("IsFall", false);
    }
}
