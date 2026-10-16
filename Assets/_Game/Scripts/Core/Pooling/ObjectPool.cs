using System;
using System.Collections.Generic;
using SerpentRush.Core.Factories;

namespace SerpentRush.Core.Pooling
{
    /// <summary>
    /// Stack pool for reference types. Not thread-safe.
    /// New instances come from <see cref="IFactory{T}"/>. Inactive instances above <c>maxInactive</c> are destroyed instead of stored.
    /// </summary>
    public sealed class ObjectPool<T> : IObjectPool<T> where T : class
    {
        readonly IFactory<T> _factory;
        readonly Action<T> _onGet;
        readonly Action<T> _onRelease;
        readonly Action<T> _onDestroy;
        readonly Stack<T> _inactive;
        readonly HashSet<T> _active;
        readonly int _maxInactive;

        public int CountInactive => _inactive.Count;

        public int CountActive => _active.Count;

        public ObjectPool(
            Func<T> create,
            Action<T> onGet = null,
            Action<T> onRelease = null,
            Action<T> onDestroy = null,
            int initialCapacity = 0,
            int maxInactive = 32)
            : this(Wrap(create), onGet, onRelease, onDestroy, initialCapacity, maxInactive)
        {
        }

        public ObjectPool(
            IFactory<T> factory,
            Action<T> onGet = null,
            Action<T> onRelease = null,
            Action<T> onDestroy = null,
            int initialCapacity = 0,
            int maxInactive = 32)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            if (initialCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(initialCapacity));
            if (maxInactive < 0)
                throw new ArgumentOutOfRangeException(nameof(maxInactive));
            if (initialCapacity > maxInactive)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(initialCapacity),
                    "Initial capacity cannot exceed max inactive.");
            }

            _factory = factory;
            _onGet = onGet;
            _onRelease = onRelease;
            _onDestroy = onDestroy;
            _maxInactive = maxInactive;
            _inactive = new Stack<T>(initialCapacity);
            _active = new HashSet<T>();

            for (int i = 0; i < initialCapacity; i++)
                _inactive.Push(Create());
        }

        public T Get()
        {
            var item = _inactive.Count > 0 ? _inactive.Pop() : Create();
            _active.Add(item);
            _onGet?.Invoke(item);
            return item;
        }

        public void Release(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (!_active.Remove(item))
                throw new InvalidOperationException("Release called with an instance that is not active in this pool.");

            _onRelease?.Invoke(item);

            if (_inactive.Count < _maxInactive)
                _inactive.Push(item);
            else
                _onDestroy?.Invoke(item);
        }

        public void Clear()
        {
            foreach (var item in _active)
                _onDestroy?.Invoke(item);

            _active.Clear();

            while (_inactive.Count > 0)
                _onDestroy?.Invoke(_inactive.Pop());
        }

        T Create()
        {
            var item = _factory.Create();
            if (item == null)
                throw new InvalidOperationException("Pool factory returned null.");

            return item;
        }

        static IFactory<T> Wrap(Func<T> create)
        {
            if (create == null)
                throw new ArgumentNullException(nameof(create));

            return new DelegateFactory<T>(create);
        }
    }
}
