namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Suspended run. Scaled time is frozen by the flow policy while this state is active.
    /// </summary>
    public sealed class PausedState : GameState
    {
        public override GameStateId Id => GameStateId.Paused;
    }

    public sealed class PausedStateBuilder : IGameStateBuilder
    {
        public GameStateId Id => GameStateId.Paused;

        public GameState Create(GameStateContext context)
        {
            return new PausedState();
        }
    }
}
