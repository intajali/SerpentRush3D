using System;
using System.Collections.Generic;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Registry of <see cref="IGameStateBuilder"/> instances. This is the only type that selects a builder.
    /// </summary>
    public sealed class GameStateFactory : IGameStateFactory
    {
        readonly Dictionary<GameStateId, IGameStateBuilder> _builders;

        public GameStateFactory(IEnumerable<IGameStateBuilder> builders)
        {
            if (builders == null)
                throw new ArgumentNullException(nameof(builders));

            _builders = new Dictionary<GameStateId, IGameStateBuilder>();

            foreach (var builder in builders)
            {
                if (builder == null)
                    throw new ArgumentException("State builders cannot contain null.", nameof(builders));

                if (_builders.ContainsKey(builder.Id))
                    throw new ArgumentException($"A builder for {builder.Id} is already registered.", nameof(builders));

                _builders.Add(builder.Id, builder);
            }
        }

        public GameState Create(GameStateId id, GameStateContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (!_builders.TryGetValue(id, out var builder))
                throw new InvalidOperationException($"No state builder is registered for {id}.");

            var state = builder.Create(context);
            if (state == null)
                throw new InvalidOperationException($"The builder for {id} returned null.");

            if (state.Id != id)
                throw new InvalidOperationException($"The builder for {id} returned a {state.Id} state.");

            return state;
        }
    }
}
