namespace SerpentRush.Core.StateMachine
{
    /// <summary>
    /// One step in a <see cref="StateMachine{T}"/>. Enter and Exit bracket the active period; Tick runs once per update.
    /// </summary>
    public interface IState
    {
        void Enter();

        void Tick(float deltaTime);

        void Exit();
    }
}
