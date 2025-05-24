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

            foreach (var state in states)
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

        public void Update(float deltaTime) =>
            _currentState?.Update(deltaTime);

        public void Reset() =>
            Switch(_states.First());
    }
}