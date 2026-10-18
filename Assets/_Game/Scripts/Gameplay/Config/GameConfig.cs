using UnityEngine;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Design-time session settings. Assign an asset on <c>GameBootstrapper</c>.
    /// Arena size and pool capacities are stored for later systems and are not read yet.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Serpent Rush/Game Config")]
    public sealed class GameConfig : ScriptableObject, IGameConfig
    {
        [Header("Session")]
        [Tooltip("Used when VSync Count is 0. Ignored while VSync is enabled.")]
        [SerializeField, Min(1)] int _targetFrameRate = 60;

        [Tooltip("0 disables VSync and applies Target Frame Rate. 1 enables VSync.")]
        [SerializeField, Range(0, 1)] int _vSyncCount = 1;

        [Tooltip("Writes allowed and rejected state changes to the Console.")]
        [SerializeField] bool _logStateTransitions = true;

        [Tooltip("Starts the run after bootstrap. Turn this off when a menu screen is what starts the run.")]
        [SerializeField] bool _startInPlaying = true;

        [Header("World")]
        [Tooltip("Half the arena width and depth in world units. Not read by gameplay yet.")]
        [SerializeField, Min(1f)] float _arenaHalfExtent = 25f;

        [Header("Pools")]
        [Tooltip("Planned prewarm size for body segments. Not read by gameplay yet.")]
        [SerializeField, Min(0)] int _segmentPoolCapacity = 64;

        [Tooltip("Planned prewarm size for pickups. Not read by gameplay yet.")]
        [SerializeField, Min(0)] int _pickupPoolCapacity = 32;

        public int TargetFrameRate => _targetFrameRate;

        public int VSyncCount => _vSyncCount;

        public bool LogStateTransitions => _logStateTransitions;

        public bool StartInPlaying => _startInPlaying;

        public float ArenaHalfExtent => _arenaHalfExtent;

        public int SegmentPoolCapacity => _segmentPoolCapacity;

        public int PickupPoolCapacity => _pickupPoolCapacity;

        void OnValidate()
        {
            _targetFrameRate = Mathf.Max(1, _targetFrameRate);
            _vSyncCount = Mathf.Clamp(_vSyncCount, 0, 1);
            _arenaHalfExtent = Mathf.Max(1f, _arenaHalfExtent);
            _segmentPoolCapacity = Mathf.Max(0, _segmentPoolCapacity);
            _pickupPoolCapacity = Mathf.Max(0, _pickupPoolCapacity);
        }
    }
}
