using UnityEngine;

public abstract class PlayerStateBase
{
    protected PlayerStateMachine _stateMachine;
    protected PlayerController _controller;

    protected Rigidbody2D _rb;
    protected Animator _ani;


    public PlayerStateBase(PlayerController controller)
    {
        _controller = controller;

        _stateMachine = controller.StateMachine;

        _rb = controller.Rb;

        _ani = controller.Ani;
    }


    public virtual void Enter()
    {
    }


    public virtual void Update()
    {
    }


    public virtual void FixedUpdate()
    {
    }


    public virtual void Exit()
    {
    }
}