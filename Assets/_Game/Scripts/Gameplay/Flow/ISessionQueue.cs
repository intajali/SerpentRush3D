namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Narrow port a state uses to ask for a later transition.
    /// States do not receive the session itself.
    /// </summary>
    public interface ISessionQueue
    {
        void QueueTransition(GameStateId id, string reason = null);
    }
}
