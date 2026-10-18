using System.Collections.Generic;
using SerpentRush.Core.Pooling;
using UnityEngine;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Keeps a field of pooled energy orbs and tells <see cref="SnakeGrowthController"/> when one is collected.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Serpent Rush/Collectable Spawner")]
    public sealed class CollectableSpawner : MonoBehaviour
    {
        [SerializeField] EnergyConfig _config;
        [SerializeField] EnergyCollectable _prefab;
        [SerializeField] SnakeGrowthController _growth;
        [SerializeField] Transform _snake;

        readonly List<EnergyCollectable> _active = new List<EnergyCollectable>(32);

        GameObjectPool _pool;
        Transform _poolRoot;
        bool _ready;

        void Start()
        {
            if (_config == null || _prefab == null)
            {
                Debug.LogError("[SerpentRush] CollectableSpawner needs an Energy Config and an Energy Collectable prefab.", this);
                return;
            }

            if (_growth == null)
                _growth = FindAnyObjectByType<SnakeGrowthController>();

            if (_snake == null && _growth != null)
                _snake = _growth.transform;

            var poolObject = new GameObject("Energy Pool");
            _poolRoot = poolObject.transform;

            int count = _config.ActiveCount;
            int capacity = Mathf.Max(_config.PoolCapacity, count + 8, 1);
            _pool = new GameObjectPool(_prefab.gameObject, count, capacity, _poolRoot);

            for (int i = 0; i < count; i++)
                SpawnOne();

            _ready = true;
        }

        void OnDestroy()
        {
            _ready = false;
            _active.Clear();

            if (_pool != null)
            {
                _pool.Clear();
                _pool.Dispose();
                _pool = null;
            }

            if (_poolRoot != null)
                Destroy(_poolRoot.gameObject);
        }

        void SpawnOne()
        {
            var type = RollType();
            var position = FindPosition();
            var instance = _pool.Get(position, Quaternion.identity);
            var orb = instance.GetComponent<EnergyCollectable>();
            if (orb == null)
            {
                Debug.LogError("[SerpentRush] Energy prefab is missing EnergyCollectable on its root.", _prefab);
                _pool.Release(instance);
                return;
            }

            orb.Prepare(
                type,
                _config.ValueOf(type),
                _config.DiameterOf(type),
                _config.ColorOf(type),
                _config.FeedbackDuration,
                position,
                OnCollected,
                OnFeedbackFinished);

            _active.Add(orb);
        }

        void OnCollected(EnergyCollectable orb)
        {
            if (!_ready || orb == null)
                return;

            _active.Remove(orb);
            if (_growth != null)
                _growth.AddEnergy(orb.Type, orb.Value);

            orb.PlayFeedback();
            SpawnOne();
        }

        void OnFeedbackFinished(EnergyCollectable orb)
        {
            if (orb == null || _pool == null)
                return;

            _pool.Release(orb.gameObject);
        }

        EnergyType RollType()
        {
            float common = _config.WeightOf(EnergyType.Common);
            float rare = _config.WeightOf(EnergyType.Rare);
            float epic = _config.WeightOf(EnergyType.Epic);
            float total = common + rare + epic;
            if (total <= 0f)
                return EnergyType.Common;

            float roll = Random.value * total;
            if (roll < common)
                return EnergyType.Common;

            if (roll < common + rare)
                return EnergyType.Rare;

            return EnergyType.Epic;
        }

        Vector3 FindPosition()
        {
            float extent = _config.SpawnHalfExtent;
            float spacing = _config.MinimumSpacing;
            float clearance = _config.ClearanceFromSnake;
            float spacingSqr = spacing * spacing;
            float clearanceSqr = clearance * clearance;
            var snake = _snake != null ? _snake.position : Vector3.zero;

            for (int attempt = 0; attempt < 24; attempt++)
            {
                var position = new Vector3(
                    Random.Range(-extent, extent),
                    _config.SpawnHeight,
                    Random.Range(-extent, extent));

                var fromSnake = position - snake;
                fromSnake.y = 0f;
                if (fromSnake.sqrMagnitude < clearanceSqr)
                    continue;

                bool crowded = false;
                for (int i = 0; i < _active.Count; i++)
                {
                    var gap = _active[i].transform.position - position;
                    gap.y = 0f;
                    if (gap.sqrMagnitude < spacingSqr)
                    {
                        crowded = true;
                        break;
                    }
                }

                if (!crowded)
                    return position;
            }

            return new Vector3(Random.Range(-extent, extent), _config.SpawnHeight, Random.Range(-extent, extent));
        }
    }
}
