namespace AI
{
    using System.Collections.Generic;

    public class StateMachine
    {
        private IState _currentState;
        private readonly Dictionary<System.Type, IState> _states = new Dictionary<System.Type, IState>();

        public IState CurrentState => _currentState;

        public void RegisterState(IState state)
        {
            _states[state.GetType()] = state;
        }

        public void ChangeState<T>() where T : IState
        {
            System.Type type = typeof(T);
            if (!_states.ContainsKey(type)) return;

            _currentState?.OnExit();
            _currentState = _states[type];
            _currentState.OnEnter();
        }

        public void Update()
        {
            _currentState?.Tick();
        }
    }
}