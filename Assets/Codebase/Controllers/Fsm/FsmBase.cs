using System;
using System.Collections.Generic;
using System.Linq;
using Codebase.Controllers.Fsm.States;

namespace Codebase.Controllers.Fsm
{
    public abstract class FsmBase : IStateSwitcher
    {
        private readonly List<IState> _states;

        private IState _currentState;

        protected FsmBase(List<IState> states)
        {
            _states = states ?? throw new ArgumentNullException(nameof(states));
        }

        public void Initialize()
        {
            foreach (var state in _states)
                state.Initialize(this);
        }

        public void Switch(IState state)
        {
            if (_states.Contains(state) == false)
                throw new ArgumentException(nameof(state));

            _currentState?.Exit();
            _currentState = state;
            _currentState.Enter();
        }

        public void Switch<T>() where T : class, IState
        {
            var state = _states.OfType<T>().FirstOrDefault();

            if (state == null || _states.Contains(state) == false)
                throw new Exception(nameof(state));

            _currentState?.Exit();
            _currentState = state;
            _currentState.Enter();
        }

        public void Update(float deltaTime) =>
            _currentState?.Update(deltaTime);

        public void Reset() =>
            Switch(_states.First());

        // Метод для получения состояния по типу
        public T GetState<T>() where T : class, IState
        {
            return _states.OfType<T>().FirstOrDefault();
        }

        // Метод для получения текущего состояния
        public IState GetCurrentState() => _currentState;

        // Метод для получения всех состояний
        public IReadOnlyList<IState> GetAllStates() => _states.AsReadOnly();
    }
}