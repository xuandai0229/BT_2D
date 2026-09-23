using UnityEngine;

public class PlayerRunState : AgentStateBase
{
    private readonly PlayerController _player;

    public PlayerRunState(PlayerController player)
        : base(player)
    {
        _player = player;
    }

    public override void Enter()
    {
        base.Enter();
        _ani.SetBool("IsRun", true);
    }

    public override void Update()
    {
        base.Update();

        if (_player.IsGrounded() && _player.ConsumeJumpPressed())
        {
            _stateMachine.ChangeState(_player.JumpState);
            return;
        }

        if (_player.ConsumeAttackPressed())
        {
            _stateMachine.ChangeState(_player.AttackState);
            return;
        }

        if (!_player.IsGrounded())
        {
            _stateMachine.ChangeState(_player.FallState);
            return;
        }

        if (!_player.HasMoveInput)
        {
            _stateMachine.ChangeState(_player.IdleState);
            return;
        }

        _player.SetFacingDirection(_player.MoveX);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        _rb.linearVelocity = new Vector2(_player.MoveX * _player.RunSpeed, _rb.linearVelocity.y);
    }

    public override void Exit()
    {
        base.Exit();
        _ani.SetBool("IsRun", false);
    }
}