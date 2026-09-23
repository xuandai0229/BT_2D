using UnityEngine;

public abstract class AgentController : MonoBehaviour
{
    protected Rigidbody2D _rb;
    public Rigidbody2D Rb => _rb;

    protected Animator _ani;
    public Animator Ani => _ani;

    protected StateMachine _stateMachine;
    public StateMachine StateMachine => _stateMachine;

    private bool _animationEvent;

    public bool AnimationEvent
    {
        get => _animationEvent;
        set => _animationEvent = value;
    }

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        _ani = GetComponent<Animator>();

        if (_ani == null)
        {
            _ani = GetComponentInChildren<Animator>();
        }

        _stateMachine = new StateMachine();
    }

    public void TriggerAnimationEvent()
    {
        _animationEvent = true;
    }
}
