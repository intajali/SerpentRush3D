using System.Collections.Generic;
using SerpentRush.Core.Events;
using UnityEngine;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Turns collected energy into body segments through <see cref="SnakeBodyController"/>.
    /// The total lasts for the current run only.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SnakeBodyController))]
    [AddComponentMenu("Serpent Rush/Snake Growth Controller")]
    public sealed class SnakeGrowthController : MonoBehaviour
    {
        [SerializeField] SnakeBodyController _body;
        [SerializeField] EnergyConfig _config;

        IEventBus _bus;
        int _totalEnergy;
        bool _reportedMissingConfig;

        public int TotalEnergy => _totalEnergy;

        public event System.Action<EnergyChanged> Changed;

        void Awake()
        {
            if (_body == null)
                _body = GetComponent<SnakeBodyController>();
        }

        public void Bind(IEventBus bus)
        {
            _bus = bus;
            Publish(0, 0, EnergyType.Common);
        }

        public void ResetRun()
        {
            _totalEnergy = 0;
            Publish(0, 0, EnergyType.Common);
        }

        public void AddEnergy(EnergyType type, int amount)
        {
            if (amount <= 0)
                return;

            if (_config == null)
            {
                if (!_reportedMissingConfig)
                {
                    _reportedMissingConfig = true;
                    Debug.LogError("[SerpentRush] SnakeGrowthController is missing an Energy Config.", this);
                }

                return;
            }

            int threshold = Mathf.Max(1, _config.GrowthThreshold);
            int segmentsBefore = _totalEnergy / threshold;
            _totalEnergy += amount;
            int segmentsRequested = (_totalEnergy / threshold) - segmentsBefore;
            int segmentsGained = 0;

            for (int i = 0; i < segmentsRequested; i++)
            {
                if (_body == null || !_body.TryAddSegment())
                    break;

                segmentsGained++;
            }

            Publish(amount, segmentsGained, type);
        }

        void Publish(int amountGained, int segmentsGained, EnergyType source)
        {
            int threshold = _config != null ? Mathf.Max(1, _config.GrowthThreshold) : 1;
            var payload = new EnergyChanged(
                _totalEnergy,
                amountGained,
                _totalEnergy % threshold,
                threshold,
                segmentsGained,
                source);

            Changed?.Invoke(payload);
            _bus?.Publish(payload);
        }
    }
}
