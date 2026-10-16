using System;

namespace SerpentRush.Core.Events
{
    /// <summary>
    /// Observer channel. Publishers and subscribers depend on this, not on a shared static.
    /// Payloads are structs. Delivery is synchronous and follows subscription order.
    /// </summary>
    public interface IEventBus
    {
        void Subscribe<T>(Action<T> handler) where T : struct;

        void Unsubscribe<T>(Action<T> handler) where T : struct;

        void Publish<T>(T payload) where T : struct;
    }
}
