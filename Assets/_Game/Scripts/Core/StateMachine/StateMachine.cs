using System;

namespace SerpentRush.Core.StateMachine
{
    /// <summary>
    /// Single-current-state machine. Transitions are synchronous and cannot be nested.
    /// Callers that need to react during Enter or Exit should queue the next state and apply it afterwards.
    /// </summary>
    public sealed class StateMachine<T> : IStateMachine<T> where T : class, IState
    {
        T _current;
        bool _isTransitioning;

        public T Current => _current;

        public bool HasState => _current != null;

        public void ChangeState(T next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            if (_isTransitioning)
            {
                throw new InvalidOperationException(
                    "State changes cannot be nested. Queue the next state and apply it after the current transition.");
            }

            if (ReferenceEquals(_current, next))
                return;

            _isTransitioning = true;
            try
            {
                _current?.Exit();
                _current = next;
                _current.Enter();
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        public void Tick(float deltaTime)
        {
            _current?.Tick(deltaTime);
        }

        public void Stop()
        {
            if (_current == null)
                return;

            if (_isTransitioning)
                throw new InvalidOperationException("Cannot stop the state machine during a transition.");

            _isTransitioning = true;
            try
            {
                _current.Exit();
                _current = null;
            }
            finally
            {
                _isTransitioning = false;
            }
        }
    }
}
