namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Legal session transitions and the time scale each state uses.
    /// Paused and Game Over freeze scaled time. The state machine itself ticks with unscaled time.
    /// </summary>
    public sealed class GameFlowRules : IGameFlowPolicy
    {
        public bool CanTransition(GameStateId from, GameStateId to)
        {
            switch (from)
            {
                case GameStateId.None:
                    return to == GameStateId.Bootstrap;
                case GameStateId.Bootstrap:
                    return to == GameStateId.MainMenu;
                case GameStateId.MainMenu:
                    return to == GameStateId.Playing;
                case GameStateId.Playing:
                    return to == GameStateId.Paused
                        || to == GameStateId.GameOver
                        || to == GameStateId.MainMenu;
                case GameStateId.Paused:
                    return to == GameStateId.Playing
                        || to == GameStateId.MainMenu;
                case GameStateId.GameOver:
                    return to == GameStateId.Playing
                        || to == GameStateId.MainMenu;
                default:
                    return false;
            }
        }

        public float TimeScaleFor(GameStateId state)
        {
            switch (state)
            {
                case GameStateId.Paused:
                case GameStateId.GameOver:
                    return 0f;
                default:
                    return 1f;
            }
        }
    }
}
