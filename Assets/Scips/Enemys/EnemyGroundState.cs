public abstract class EnemyGroundState : AgentStateBase
{
    protected readonly EnemyController _controller;

    protected float _stateTimer;

    protected bool HasExited { get; private set; }


    public EnemyGroundState(EnemyController controller) : base(controller)
    {
        _controller = controller;
    }


    public override void Enter()
    {
        base.Enter();

        HasExited = false;
    }


    public override void Update()
    {
        base.Update();
    }


    public override void Exit()
    {
        base.Exit();

        HasExited = true;
    }
}