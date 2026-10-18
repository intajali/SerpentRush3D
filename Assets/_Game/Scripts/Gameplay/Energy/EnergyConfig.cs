using UnityEngine;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Tuning for energy orbs and how many points add a body segment.
    /// </summary>
    [CreateAssetMenu(fileName = "EnergyConfig", menuName = "Serpent Rush/Energy Config")]
    public sealed class EnergyConfig : ScriptableObject, IEnergyConfig
    {
        [SerializeField, Min(1)] int _commonValue = 1;
        [SerializeField, Min(1)] int _rareValue = 5;
        [SerializeField, Min(1)] int _epicValue = 10;

        [Tooltip("Energy points required for one new body segment. Leftover points carry toward the next one.")]
        [SerializeField, Min(1)] int _growthThreshold = 5;

        [SerializeField, Min(0.05f)] float _commonDiameter = 0.7f;
        [SerializeField, Min(0.05f)] float _rareDiameter = 0.95f;
        [SerializeField, Min(0.05f)] float _epicDiameter = 1.2f;

        [SerializeField] Color _commonColor = new Color(0.95f, 0.82f, 0.2f, 1f);
        [SerializeField] Color _rareColor = new Color(0.25f, 0.75f, 1f, 1f);
        [SerializeField] Color _epicColor = new Color(0.72f, 0.28f, 1f, 1f);

        [SerializeField, Min(0f)] float _commonWeight = 70f;
        [SerializeField, Min(0f)] float _rareWeight = 25f;
        [SerializeField, Min(0f)] float _epicWeight = 5f;

        [SerializeField, Min(1)] int _activeCount = 18;
        [SerializeField, Min(1f)] float _spawnHalfExtent = 18f;
        [SerializeField] float _spawnHeight = 0.55f;
        [SerializeField, Min(0.1f)] float _minimumSpacing = 2.5f;
        [SerializeField, Min(0f)] float _clearanceFromSnake = 4f;
        [SerializeField, Min(0f)] float _feedbackDuration = 0.22f;
        [SerializeField, Min(1)] int _poolCapacity = 32;

        public int CommonValue => _commonValue;

        public int RareValue => _rareValue;

        public int EpicValue => _epicValue;

        public int GrowthThreshold => _growthThreshold;

        public int ActiveCount => _activeCount;

        public float SpawnHalfExtent => _spawnHalfExtent;

        public float SpawnHeight => _spawnHeight;

        public float MinimumSpacing => _minimumSpacing;

        public float ClearanceFromSnake => _clearanceFromSnake;

        public float FeedbackDuration => _feedbackDuration;

        public int PoolCapacity => _poolCapacity;

        public int ValueOf(EnergyType type)
        {
            switch (type)
            {
                case EnergyType.Rare:
                    return _rareValue;
                case EnergyType.Epic:
                    return _epicValue;
                default:
                    return _commonValue;
            }
        }

        public float DiameterOf(EnergyType type)
        {
            switch (type)
            {
                case EnergyType.Rare:
                    return _rareDiameter;
                case EnergyType.Epic:
                    return _epicDiameter;
                default:
                    return _commonDiameter;
            }
        }

        public Color ColorOf(EnergyType type)
        {
            switch (type)
            {
                case EnergyType.Rare:
                    return _rareColor;
                case EnergyType.Epic:
                    return _epicColor;
                default:
                    return _commonColor;
            }
        }

        public float WeightOf(EnergyType type)
        {
            switch (type)
            {
                case EnergyType.Rare:
                    return _rareWeight;
                case EnergyType.Epic:
                    return _epicWeight;
                default:
                    return _commonWeight;
            }
        }

        void OnValidate()
        {
            _commonValue = Mathf.Max(1, _commonValue);
            _rareValue = Mathf.Max(1, _rareValue);
            _epicValue = Mathf.Max(1, _epicValue);
            _growthThreshold = Mathf.Max(1, _growthThreshold);
            _commonDiameter = Mathf.Max(0.05f, _commonDiameter);
            _rareDiameter = Mathf.Max(0.05f, _rareDiameter);
            _epicDiameter = Mathf.Max(0.05f, _epicDiameter);
            _commonWeight = Mathf.Max(0f, _commonWeight);
            _rareWeight = Mathf.Max(0f, _rareWeight);
            _epicWeight = Mathf.Max(0f, _epicWeight);
            _activeCount = Mathf.Max(1, _activeCount);
            _spawnHalfExtent = Mathf.Max(1f, _spawnHalfExtent);
            _minimumSpacing = Mathf.Max(0.1f, _minimumSpacing);
            _clearanceFromSnake = Mathf.Max(0f, _clearanceFromSnake);
            _feedbackDuration = Mathf.Max(0f, _feedbackDuration);
            _poolCapacity = Mathf.Max(1, _poolCapacity);
        }
    }
}
