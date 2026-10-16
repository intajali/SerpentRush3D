using System;
using System.Collections.Generic;
using UnityEngine;

namespace SerpentRush.Core.Pooling
{
    /// <summary>
    /// Prefab pool built on <see cref="ObjectPool{T}"/>.
    /// The parent should outlive every leased instance. Releasing reparents inactive objects under that parent.
    /// </summary>
    public sealed class GameObjectPool : IGameObjectPool
    {
        readonly GameObject _prefab;
        readonly Transform _parent;
        readonly IObjectPool<GameObject> _pool;
        readonly List<IPoolable> _poolables = new List<IPoolable>(4);

        Vector3 _spawnPosition;
        Quaternion _spawnRotation;
        bool _hasSpawnPose;

        public int CountActive => _pool.CountActive;

        public int CountInactive => _pool.CountInactive;

        public GameObjectPool(GameObject prefab, int initialCapacity = 0, int maxInactive = 32, Transform parent = null)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            _prefab = prefab;
            _parent = parent;
            _pool = new ObjectPool<GameObject>(
                CreateInstance,
                OnGet,
                OnRelease,
                DestroyInstance,
                initialCapacity,
                maxInactive);
        }

        public GameObject Get()
        {
            _hasSpawnPose = false;
            return _pool.Get();
        }

        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            _spawnPosition = position;
            _spawnRotation = rotation;
            _hasSpawnPose = true;
            return _pool.Get();
        }

        public void Release(GameObject instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            _pool.Release(instance);
        }

        public void Clear()
        {
            _pool.Clear();
        }

        public void Dispose()
        {
            Clear();
        }

        GameObject CreateInstance()
        {
            var instance = UnityEngine.Object.Instantiate(_prefab, _parent);
            instance.SetActive(false);
            return instance;
        }

        void OnGet(GameObject instance)
        {
            if (_hasSpawnPose)
                instance.transform.SetPositionAndRotation(_spawnPosition, _spawnRotation);

            _hasSpawnPose = false;
            instance.SetActive(true);
            Notify(instance, spawned: true);
        }

        void OnRelease(GameObject instance)
        {
            Notify(instance, spawned: false);
            instance.SetActive(false);

            if (_parent != null)
                instance.transform.SetParent(_parent, false);
        }

        static void DestroyInstance(GameObject instance)
        {
            if (instance != null)
                UnityEngine.Object.Destroy(instance);
        }

        void Notify(GameObject instance, bool spawned)
        {
            instance.GetComponents(_poolables);

            for (int i = 0; i < _poolables.Count; i++)
            {
                if (spawned)
                    _poolables[i].OnSpawned();
                else
                    _poolables[i].OnDespawned();
            }

            _poolables.Clear();
        }
    }
}
