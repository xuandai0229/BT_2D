using UnityEngine;

public class StateMachine
{
    private AgentStateBase _currentState;

    public AgentStateBase CurrentState => _currentState;

    private bool _isShutDown;

    public void ChangeState(AgentStateBase newState)
    {
        if (newState == null ||
            newState == _currentState ||
            _isShutDown)
        {
            return;
        }

        _currentState?.Exit();

        _currentState = newState;

        _currentState?.Enter();
    }

    public void Update()
    {
        if (_isShutDown)
            return;

        _currentState?.Update();
    }

    public void FixedUpdate()
    {
        if (_isShutDown)
            return;

        _currentState?.FixedUpdate();
    }

    public void Shutdown()
    {
        _isShutDown = true;
    }
}
