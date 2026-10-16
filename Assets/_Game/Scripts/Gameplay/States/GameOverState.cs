namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Terminal run state. The reason is supplied by the transition that entered this state.
    /// </summary>
    public sealed class GameOverState : GameState, IGameOverDetails
    {
        public override GameStateId Id => GameStateId.GameOver;

        public string Reason { get; private set; } = string.Empty;

        public void SetReason(string reason)
        {
            Reason = reason ?? string.Empty;
        }

        public override void Exit()
        {
            Reason = string.Empty;
        }
    }

    public sealed class GameOverStateBuilder : IGameStateBuilder
    {
        public GameStateId Id => GameStateId.GameOver;

        public GameState Create(GameStateContext context)
        {
            return new GameOverState();
        }
    }
}
