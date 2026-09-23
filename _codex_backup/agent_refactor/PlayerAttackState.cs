using UnityEngine;

public class PlayerAttackState : PlayerStateBase
{
    private const int MAX_COMBO_INDEX = 4;

    private static readonly int AttackIndexHash = Animator.StringToHash("AttackIndex");

    private int _attackIndex;

    private bool _nextAttackQueued;


    public PlayerAttackState(PlayerController controller)
        : base(controller)
    {
    }


    public override void Enter()
    {
        base.Enter();


        _attackIndex = 1;


        _nextAttackQueued = false;


        _controller.StopHorizontal();


        if (_controller.HasMoveInput)
        {
            _controller.SetFacingDirection( _controller.MoveX);
        }


        _ani.SetBool("IsRun", false);
        _ani.SetBool("IsJump", false);
        _ani.SetBool("IsFall", false);


        _ani.SetBool("IsAttack", true);
        _ani.SetInteger( AttackIndexHash, _attackIndex );
    }


    public override void Update()
    {
        base.Update();

        if (_controller.ConsumeAttackPressed())
        {
            if (_attackIndex < MAX_COMBO_INDEX)
            {
                _nextAttackQueued = true;
            }
        }


        if (!_controller.AnimationEvent)
        {
            return;
        }

        _controller.AnimationEvent = false;


        if (_nextAttackQueued &&_attackIndex < MAX_COMBO_INDEX)
        {
            _attackIndex++;
            _nextAttackQueued = false;

            _ani.SetInteger( AttackIndexHash,_attackIndex);

            return;
        }

        FinishCombo();
    }


    public override void FixedUpdate()
    {
        base.FixedUpdate();

        _controller.StopHorizontal();
    }


    public override void Exit()
    {
        base.Exit();

        _ani.SetBool("IsAttack", false);

        _ani.SetInteger(AttackIndexHash,0 );

        _attackIndex = 0;
        _nextAttackQueued = false;
    }


    private void FinishCombo()
    {
        _ani.SetBool("IsAttack", false);

        _ani.SetInteger(AttackIndexHash, 0 );

        if (!_controller.IsGrounded())
        {
            _stateMachine.ChangeState(_controller.FallState);

            return;
        }

        if (_controller.HasMoveInput)
        {
            _stateMachine.ChangeState(_controller.RunState);

            return;
        }
        _stateMachine.ChangeState(_controller.IdleState);
    }
}
