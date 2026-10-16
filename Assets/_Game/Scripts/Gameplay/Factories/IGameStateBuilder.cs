namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Factory method for one game state. Adding a state means adding a builder, not editing existing states.
    /// </summary>
    public interface IGameStateBuilder
    {
        GameStateId Id { get; }

        GameState Create(GameStateContext context);
    }
}
