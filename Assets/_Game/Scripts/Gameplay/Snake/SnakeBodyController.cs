using System.Collections.Generic;
using SerpentRush.Core.Pooling;
using UnityEngine;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Builds the body behind the object <see cref="SnakeMovement"/> moves.
    /// Each segment samples the head's recorded path by arc length. Segments are not parented to each other.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SnakeMovement))]
    [AddComponentMenu("Serpent Rush/Snake Body Controller")]
    public sealed class SnakeBodyController : MonoBehaviour
    {
        [SerializeField] SnakeBodyConfig _config;

        [Tooltip("Prefab with a Snake Segment on the root. No Rigidbody.")]
        [SerializeField] SnakeSegment _segmentPrefab;

        readonly List<SnakeSegment> _segments = new List<SnakeSegment>(64);
        readonly MovementPathRecorder _path = new MovementPathRecorder();

        GameObjectPool _pool;
        Transform _poolRoot;
        bool _ready;
        bool _reportedMissingSetup;

        public int SegmentCount => _segments.Count;

        public ISnakeBodyConfig Config => _config;

        void Start()
        {
            if (_config == null || _segmentPrefab == null)
            {
                ReportMissingSetup();
                return;
            }

            var poolObject = new GameObject("Snake Segment Pool");
            _poolRoot = poolObject.transform;

            int inactive = Mathf.Max(_config.PoolCapacity, _config.InitialSegmentCount, 1);
            _pool = new GameObjectPool(_segmentPrefab.gameObject, _config.InitialSegmentCount, inactive, _poolRoot);

            float tail = TailDistance(_config.InitialSegmentCount);
            _path.Seed(transform.position, transform.forward, Mathf.Max(tail, _config.PathSampleSpacing), _config.PathSampleSpacing);
            _path.RetainDistance = RetainFor(tail);

            for (int i = 0; i < _config.InitialSegmentCount; i++)
                _segments.Add(SpawnAt(i));

            _ready = true;
        }

        void LateUpdate()
        {
            if (!_ready)
                return;

            float tail = TailDistance(_segments.Count);
            _path.SampleSpacing = _config.PathSampleSpacing;
            _path.RetainDistance = RetainFor(tail);
            _path.Advance(transform.position);
            _path.EnsureSpan(tail);
            ApplyBodySize();
            PlaceSegments();
        }

        void OnDestroy()
        {
            _ready = false;
            _segments.Clear();

            if (_pool != null)
            {
                _pool.Clear();
                _pool.Dispose();
                _pool = null;
            }

            if (_poolRoot != null)
                Destroy(_poolRoot.gameObject);
        }

        [ContextMenu("Add Body Segment")]
        public void AddSegment()
        {
            if (!EnsureReady())
                return;

            int index = _segments.Count;
            float tail = TailDistance(index + 1);
            _path.RetainDistance = RetainFor(tail);
            _path.EnsureSpan(tail);
            _segments.Add(SpawnAt(index));
        }

        [ContextMenu("Remove Body Segment")]
        public void RemoveSegment()
        {
            if (!_ready || _segments.Count == 0)
                return;

            int last = _segments.Count - 1;
            var segment = _segments[last];
            _segments.RemoveAt(last);
            _pool.Release(segment.gameObject);
            _path.RetainDistance = RetainFor(TailDistance(_segments.Count));
        }

        SnakeSegment SpawnAt(int index)
        {
            _path.SampleBehind(_config.SegmentSpacing * (index + 1), out var position, out var tangent);
            var rotation = Heading(tangent);
            var instance = _pool.Get(position, rotation);
            var segment = instance.GetComponent<SnakeSegment>();
            if (segment == null)
            {
                Debug.LogError("[SerpentRush] Snake segment prefab is missing SnakeSegment on its root.", _segmentPrefab);
                segment = instance.AddComponent<SnakeSegment>();
            }

            segment.SetSize(_config.SegmentDiameter, SegmentLength());
            segment.Apply(position, rotation, 1f);
            return segment;
        }

        void ApplyBodySize()
        {
            float diameter = _config.SegmentDiameter;
            float length = SegmentLength();
            for (int i = 0; i < _segments.Count; i++)
                _segments[i].SetSize(diameter, length);
        }

        void PlaceSegments()
        {
            float spacing = _config.SegmentSpacing;
            for (int i = 0; i < _segments.Count; i++)
            {
                _path.SampleBehind(spacing * (i + 1), out var position, out var tangent);
                _segments[i].Apply(position, Heading(tangent), 1f);
            }
        }

        float SegmentLength()
        {
            return _config.SegmentSpacing * 0.98f;
        }

        float TailDistance(int segmentCount)
        {
            return _config.SegmentSpacing * segmentCount;
        }

        float RetainFor(float tailDistance)
        {
            return tailDistance + (_config.PathSampleSpacing * 4f);
        }

        bool EnsureReady()
        {
            if (_ready)
                return true;

            if (_config == null || _segmentPrefab == null)
                ReportMissingSetup();

            return false;
        }

        void ReportMissingSetup()
        {
            if (_reportedMissingSetup)
                return;

            _reportedMissingSetup = true;
            Debug.LogError("[SerpentRush] SnakeBodyController needs a Snake Body Config and a Snake Segment prefab.", this);
        }

        static Quaternion Heading(Vector3 tangent)
        {
            tangent = SteerAim.Planar(tangent);
            if (tangent.sqrMagnitude < 0.0001f)
                return Quaternion.identity;

            return Quaternion.LookRotation(tangent.normalized, Vector3.up);
        }
    }
}
