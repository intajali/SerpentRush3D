namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Idle session before a run. Menu presentation will listen for this state.
    /// </summary>
    public sealed class MainMenuState : GameState
    {
        public override GameStateId Id => GameStateId.MainMenu;
    }

    public sealed class MainMenuStateBuilder : IGameStateBuilder
    {
        public GameStateId Id => GameStateId.MainMenu;

        public GameState Create(GameStateContext context)
        {
            return new MainMenuState();
        }
    }
}
