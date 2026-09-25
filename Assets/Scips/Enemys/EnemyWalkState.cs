public class EnemyWalkState : EnemyGroundState
{
    public EnemyWalkState(EnemyController enemy): base(enemy)
    {
    }
    public override void Enter()
    {
        base.Enter();
        _controller.SetMoveState(1);
        if (ReachedBoundary())
        {
            _controller.FlipDirection();
        }
    }
    public override void Update()
    {
        if (ReachedBoundary())
        {
            _controller.StopMoving();
            _stateMachine.ChangeState(_controller.IdleState);
            return;
        }
        base.Update();
        if (HasExited)
        {
            return;
        }
        _controller.Walk();
    }


    private bool ReachedBoundary()
    {
        return  !_controller.IsGroundDetect || _controller.IsWallDetected;
    }
}