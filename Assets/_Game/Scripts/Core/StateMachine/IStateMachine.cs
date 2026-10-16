namespace SerpentRush.Core.StateMachine
{
    /// <summary>
    /// Owns the active <see cref="IState"/> and applies one transition at a time.
    /// </summary>
    public interface IStateMachine<T> where T : class, IState
    {
        T Current { get; }

        bool HasState { get; }

        void ChangeState(T next);

        void Tick(float deltaTime);

        void Stop();
    }
}
