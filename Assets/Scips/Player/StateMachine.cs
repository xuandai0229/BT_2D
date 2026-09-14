using UnityEngine;

public class StateMachine
{
    private AgentStateBaseTest _currentState;
    public AgentStateBaseTest CurrentState => _currentState;
    private bool _isShutdown;

    public void ChangeState(AgentStateBaseTest newState)
    {
        if (newState == null || newState == _currentState || _isShutdown)
        {
            return;
        }

        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    public void Update()
    {
        if (_isShutdown)
        {
            return;
        }
        _currentState?.Update();
    }

    public void Shutdown() => _isShutdown = true;
}




