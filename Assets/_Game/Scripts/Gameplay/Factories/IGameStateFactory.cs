namespace SerpentRush.Gameplay
{
    public interface IGameStateFactory
    {
        GameState Create(GameStateId id, GameStateContext context);
    }
}
