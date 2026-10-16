using System;
using System.Collections.Generic;

namespace SerpentRush.Core.Events
{
    /// <summary>
    /// Instance observer bus. The composition root creates one and passes it to publishers and subscribers.
    /// One channel exists per payload type. Invoke it on the Unity main thread only.
    /// </summary>
    public sealed class EventBus : IEventBus
    {
        readonly Dictionary<Type, object> _channels = new Dictionary<Type, object>();

        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            GetChannel<T>().Raised += handler;
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null)
                return;

            if (!_channels.TryGetValue(typeof(T), out var channel))
                return;

            ((Channel<T>)channel).Raised -= handler;
        }

        public void Publish<T>(T payload) where T : struct
        {
            if (!_channels.TryGetValue(typeof(T), out var channel))
                return;

            ((Channel<T>)channel).Publish(payload);
        }

        Channel<T> GetChannel<T>() where T : struct
        {
            var key = typeof(T);
            if (_channels.TryGetValue(key, out var existing))
                return (Channel<T>)existing;

            var created = new Channel<T>();
            _channels.Add(key, created);
            return created;
        }

        sealed class Channel<T> where T : struct
        {
            public event Action<T> Raised;

            public void Publish(T payload)
            {
                Raised?.Invoke(payload);
            }
        }
    }
}
