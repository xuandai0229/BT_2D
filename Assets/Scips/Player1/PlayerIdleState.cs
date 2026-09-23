using UnityEngine;

public class PlayerIdleState : AgentStateBase
{
    private readonly PlayerController _player;

    public PlayerIdleState(PlayerController player)
        : base(player)
    {
        _player = player;
    }

    public override void Enter()
    {
        base.Enter();

        _ani.SetBool("IsRun", false);
        _ani.SetBool("IsJump", false);

        _player.StopHorizontal();
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

        if (_player.HasMoveInput)
        {
            _stateMachine.ChangeState(_player.RunState);

            return;
        }
    }
}