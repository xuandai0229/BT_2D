using UnityEngine;

public class EnemyIdleState : EnemyGroundState
{
    public EnemyIdleState(EnemyController enemy): base(enemy)
    {
    }


    public override void Enter()
    {
        base.Enter();
        _controller.StopMoving();
        _controller.SetMoveState(0);
        _stateTimer = _controller.IdleTime;
    }


    public override void Update()
    {
        base.Update();
        if (HasExited)
        {
            return;
        }
        _stateTimer -= Time.deltaTime;
        if (_stateTimer < 0f)
        {
            _stateMachine.ChangeState(_controller.WalkState);
        }
    }
}