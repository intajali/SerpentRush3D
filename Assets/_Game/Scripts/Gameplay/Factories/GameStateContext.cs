using System;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Explicit dependencies handed to a state builder at creation time.
    /// </summary>
    public sealed class GameStateContext
    {
        public GameStateContext(IGameConfig config, ISessionQueue queue)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Queue = queue ?? throw new ArgumentNullException(nameof(queue));
        }

        public IGameConfig Config { get; }

        public ISessionQueue Queue { get; }
    }
}
