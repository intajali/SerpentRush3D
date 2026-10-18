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
        bool _reportedMissingConfig;

        public ISnakeMovementConfig Config => _config;

        void Awake()
        {
            _lockedHeight = transform.position.y;
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

            var position = transform.position + facing * (_config.ForwardSpeed * deltaTime);
            if (_lockPlanarHeight)
                position.y = _lockedHeight;

            transform.position = position;
        }
    }
}
