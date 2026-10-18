using UnityEngine;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Spacing, path resolution, and starting length for <see cref="SnakeBodyController"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "SnakeBodyConfig", menuName = "Serpent Rush/Snake Body Config")]
    public sealed class SnakeBodyConfig : ScriptableObject, ISnakeBodyConfig
    {
        [Tooltip("Arc length between segment centers, along the head's path.")]
        [SerializeField, Min(0.05f)] float _segmentSpacing = 0.75f;

        [Tooltip("Thickness of each body segment. Stays thinner than the spacing so the pieces do not sit inside each other.")]
        [SerializeField, Min(0.05f)] float _segmentDiameter = 0.36f;

        [Tooltip("Body segments created on play. The head is not included.")]
        [SerializeField, Min(0)] int _initialSegmentCount = 50;

        [Tooltip("Distance the head must travel before a new path point is stored. Smaller values hug tight turns more closely.")]
        [SerializeField, Min(0.01f)] float _pathSampleSpacing = 0.05f;

        [Tooltip("Inactive segment instances the pool can hold. Raised automatically when the starting length is larger.")]
        [SerializeField, Min(1)] int _poolCapacity = 80;

        public float SegmentSpacing => _segmentSpacing;

        public float SegmentDiameter => _segmentDiameter;

        public int InitialSegmentCount => _initialSegmentCount;

        public float PathSampleSpacing => _pathSampleSpacing;

        public int PoolCapacity => _poolCapacity;

        void OnValidate()
        {
            _segmentSpacing = Mathf.Max(0.05f, _segmentSpacing);
            _segmentDiameter = Mathf.Clamp(_segmentDiameter, 0.05f, _segmentSpacing * 0.85f);
            _initialSegmentCount = Mathf.Max(0, _initialSegmentCount);
            _pathSampleSpacing = Mathf.Max(0.01f, _pathSampleSpacing);
            _poolCapacity = Mathf.Max(1, _poolCapacity);
        }
    }
}
