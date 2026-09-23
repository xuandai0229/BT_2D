using UnityEngine;

public class PlayerAttackState : AgentStateBase
{
    private const int MAX_COMBO_INDEX = 4;

    private static readonly int AttackIndexHash = Animator.StringToHash("AttackIndex");

    private int _attackIndex;

    private bool _nextAttackQueued;

    private readonly PlayerController _player;

    public PlayerAttackState(PlayerController player)
        : base(player)
    {
        _player = player;
    }

    public override void Enter()
    {
        base.Enter();

        _attackIndex = 1;

        _nextAttackQueued = false;

        _player.StopHorizontal();

        if (_player.HasMoveInput)
        {
            _player.SetFacingDirection(_player.MoveX);
        }

        _ani.SetBool("IsRun", false);
        _ani.SetBool("IsJump", false);
        _ani.SetBool("IsFall", false);

        _ani.SetBool("IsAttack", true);
        _ani.SetInteger(AttackIndexHash, _attackIndex);
    }

    public override void Update()
    {
        base.Update();

        if (_player.ConsumeAttackPressed())
        {
            if (_attackIndex < MAX_COMBO_INDEX)
            {
                _nextAttackQueued = true;
            }
        }

        if (!_player.AnimationEvent)
        {
            return;
        }

        _player.AnimationEvent = false;

        if (_nextAttackQueued && _attackIndex < MAX_COMBO_INDEX)
        {
            _attackIndex++;
            _nextAttackQueued = false;

            _ani.SetInteger(AttackIndexHash, _attackIndex);

            return;
        }

        FinishCombo();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        _player.StopHorizontal();
    }

    public override void Exit()
    {
        base.Exit();

        _ani.SetBool("IsAttack", false);

        _ani.SetInteger(AttackIndexHash, 0);

        _attackIndex = 0;
        _nextAttackQueued = false;
    }

    private void FinishCombo()
    {
        _ani.SetBool("IsAttack", false);

        _ani.SetInteger(AttackIndexHash, 0);

        if (!_player.IsGrounded())
        {
            _stateMachine.ChangeState(_player.FallState);

            return;
        }

        if (_player.HasMoveInput)
        {
            _stateMachine.ChangeState(_player.RunState);

            return;
        }

        _stateMachine.ChangeState(_player.IdleState);
    }
}
