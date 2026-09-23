using UnityEngine;

public abstract class AgentStateBase
{
    protected StateMachine _stateMachine;

    protected AgentController _agent;

    protected Rigidbody2D _rb;

    protected Animator _ani;

    public AgentStateBase(AgentController agent)
    {
        _agent = agent;

        _stateMachine = agent.StateMachine;

        _rb = agent.Rb;

        _ani = agent.Ani;
    }

    public virtual void Enter()
    {
        _agent.AnimationEvent = false;
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