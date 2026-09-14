using Unity.VisualScripting;
using UnityEngine;

public class AgentStateBaseTest //POPO: 
{
    protected StateMachine _stateMachin;
    protected AgenController _controller;

    protected Rigidbody2D _rb;
    protected Animator _ani;
    public AgentStateBaseTest(PlayerController player)
    {
    }

    public virtual void Enter()
    {

    }

    public virtual void Update()
    {

    }

    public virtual void Exit()
    {
    }
}
