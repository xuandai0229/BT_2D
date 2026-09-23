using UnityEngine;

public class PlayerJumpState : AgentStateBase
{
    private readonly PlayerController _player;

    public PlayerJumpState(PlayerController player)
        : base(player)
    {
        _player = player;
    }

    public override void Enter()
    {
        base.Enter();

        _ani.SetBool("IsRun", false);
        _ani.SetBool("IsFall", false);
        _ani.SetBool("IsJump", true);

        _player.ApplyJump();
    }

    public override void Update()
    {
        base.Update();

        if (_player.HasMoveInput)
        {
            _player.SetFacingDirection(_player.MoveX);
        }

        if (_rb.linearVelocity.y <= 0f)
        {
            _stateMachine.ChangeState(_player.FallState);
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
        _ani.SetBool("IsJump", false);
    }
}