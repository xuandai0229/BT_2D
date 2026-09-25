public class EnemyRunState : EnemyGroundState
{
    public EnemyRunState(EnemyController enemy): base(enemy)
    {
    }
    public override void Enter()
    {
        base.Enter();
        _controller.SetMoveState(2);
    }
    public override void Update()
    {
        base.Update();
        if (HasExited)
        {
            return;
        }
        _controller.Run();
    }
}