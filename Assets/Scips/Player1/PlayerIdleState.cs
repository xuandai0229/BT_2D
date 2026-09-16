public class PlayerIdleState : PlayerStateBase
{
    public PlayerIdleState(PlayerController controller)
        : base(controller)
    {
    }

    public override void Enter()
    {
     
        _ani.SetBool("IsRun", false);
        _ani.SetBool("IsJump", false);

        _controller.StopHorizontal();
    }

    public override void Update()
    {

        if (_controller.IsGrounded() && _controller.ConsumeJumpPressed())
        {
            _stateMachine.ChangeState(_controller.JumpState);

            return;
        }


        if (_controller.HasMoveInput)
        {
            _stateMachine.ChangeState(_controller.RunState);

            return;
        }
    }
}