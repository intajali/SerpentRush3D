using SerpentRush.Core.StateMachine;

namespace SerpentRush.Gameplay
{
    public abstract class GameState : IState
    {
        public abstract GameStateId Id { get; }

        public virtual void Enter()
        {
        }

        public virtual void Tick(float deltaTime)
        {
        }

        public virtual void Exit()
        {
        }
    }
}
