using System;

namespace SerpentRush.Core.Factories
{
    /// <summary>
    /// Adapts a creation delegate to <see cref="IFactory{T}"/>.
    /// </summary>
    public sealed class DelegateFactory<T> : IFactory<T>
    {
        readonly Func<T> _create;

        public DelegateFactory(Func<T> create)
        {
            _create = create ?? throw new ArgumentNullException(nameof(create));
        }

        public T Create()
        {
            return _create();
        }
    }
}
