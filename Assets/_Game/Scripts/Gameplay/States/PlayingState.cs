namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Active run. Movement and arena simulation stay outside this class.
    /// </summary>
    public sealed class PlayingState : GameState
    {
        public override GameStateId Id => GameStateId.Playing;
    }

    public sealed class PlayingStateBuilder : IGameStateBuilder
    {
        public GameStateId Id => GameStateId.Playing;

        public GameState Create(GameStateContext context)
        {
            return new PlayingState();
        }
    }
}
