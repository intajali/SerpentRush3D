namespace SerpentRush.Gameplay
{
    public readonly struct PlayRequested
    {
    }

    public readonly struct PauseRequested
    {
    }

    public readonly struct ResumeRequested
    {
    }

    public readonly struct MainMenuRequested
    {
    }

    public readonly struct GameOverRequested
    {
        public GameOverRequested(string reason)
        {
            Reason = reason ?? string.Empty;
        }

        public string Reason { get; }
    }

    public readonly struct GameStateChanged
    {
        public GameStateChanged(GameStateId previous, GameStateId current)
        {
            Previous = previous;
            Current = current;
        }

        public GameStateId Previous { get; }

        public GameStateId Current { get; }
    }
}
