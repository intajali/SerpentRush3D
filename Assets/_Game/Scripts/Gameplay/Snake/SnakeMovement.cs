using UnityEngine;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Moves the head forward on the XZ plane and turns toward a desired heading at a limited rate.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Serpent Rush/Snake Movement")]
    public sealed class SnakeMovement : MonoBehaviour
    {
        [SerializeField] SnakeMovementConfig _config;

        [Tooltip("Holds the head on the height it had when play started.")]
        [SerializeField] bool _lockPlanarHeight = true;

        float _lockedHeight;
        float _runStartTime;
        bool _reportedMissingConfig;
        Rigidbody _rigidbody;

        public ISnakeMovementConfig Config => _config;

        public float CurrentSpeed
        {
            get
            {
                if (_config == null)
                    return 0f;

                float elapsed = Mathf.Max(0f, Time.time - _runStartTime);
                float speed = _config.ForwardSpeed + (elapsed * _config.SpeedIncreasePerSecond);
                return Mathf.Min(speed, _config.MaxForwardSpeed);
            }
        }

        void Awake()
        {
            _lockedHeight = transform.position.y;
            _runStartTime = Time.time;
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void ResetRun()
        {
            _runStartTime = Time.time;
        }

        public void Tick(Vector3 desiredDirection, float deltaTime)
        {
            if (_config == null)
            {
                if (!_reportedMissingConfig)
                {
                    _reportedMissingConfig = true;
                    Debug.LogError("[SerpentRush] SnakeMovement is missing a SnakeMovementConfig asset.", this);
                }

                return;
            }

            if (deltaTime <= 0f)
                return;

            var facing = SteerAim.Planar(transform.forward);
            if (facing.sqrMagnitude < 0.0001f)
                facing = Vector3.forward;
            else
                facing.Normalize();

            var desired = SteerAim.Planar(desiredDirection);
            if (desired.sqrMagnitude > 0.0001f)
            {
                desired.Normalize();
                var current = Quaternion.LookRotation(facing, Vector3.up);
                var target = Quaternion.LookRotation(desired, Vector3.up);
                var maxStep = _config.MaxTurnDegreesPerSecond * deltaTime;
                transform.rotation = Quaternion.RotateTowards(current, target, maxStep);

                facing = SteerAim.Planar(transform.forward);
                if (facing.sqrMagnitude > 0.0001f)
                    facing.Normalize();
            }
            else
            {
                transform.rotation = Quaternion.LookRotation(facing, Vector3.up);
            }

            var position = transform.position + facing * (CurrentSpeed * deltaTime);
            if (_lockPlanarHeight)
                position.y = _lockedHeight;

            if (_rigidbody != null && _rigidbody.isKinematic)
            {
                _rigidbody.rotation = transform.rotation;
                _rigidbody.position = position;
            }
            else
            {
                transform.position = position;
            }
        }
    }
}
