namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Transition policy. The session consults this instead of embedding the rules.
    /// </summary>
    public interface IGameFlowPolicy
    {
        bool CanTransition(GameStateId from, GameStateId to);

        float TimeScaleFor(GameStateId state);
    }
}
