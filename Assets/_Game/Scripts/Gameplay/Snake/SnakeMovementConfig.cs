using UnityEngine;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Tuning for the snake head. Speeds are per second so movement stays frame-rate independent.
    /// </summary>
    [CreateAssetMenu(fileName = "SnakeMovementConfig", menuName = "Serpent Rush/Snake Movement Config")]
    public sealed class SnakeMovementConfig : ScriptableObject, ISnakeMovementConfig
    {
        [Tooltip("World units travelled per second along the facing direction.")]
        [SerializeField, Min(0f)] float _forwardSpeed = 6f;

        [Tooltip("Fastest heading change, in degrees per second. About 140 at speed 6 bends in a wide arc so the head stays clear of the body.")]
        [SerializeField, Min(1f)] float _maxTurnDegreesPerSecond = 140f;

        [Tooltip("Steer input shorter than this is treated as no turn. Keyboard input is unaffected.")]
        [SerializeField, Range(0f, 0.95f)] float _inputDeadzone = 0.15f;

        [Tooltip("Maps stick and WASD axes through the camera so the keys match the screen.")]
        [SerializeField] bool _cameraRelativeSteering = true;

        public float ForwardSpeed => _forwardSpeed;

        public float MaxTurnDegreesPerSecond => _maxTurnDegreesPerSecond;

        public float InputDeadzone => _inputDeadzone;

        public bool CameraRelativeSteering => _cameraRelativeSteering;

        void OnValidate()
        {
            _forwardSpeed = Mathf.Max(0f, _forwardSpeed);
            _maxTurnDegreesPerSecond = Mathf.Max(1f, _maxTurnDegreesPerSecond);
            _inputDeadzone = Mathf.Clamp(_inputDeadzone, 0f, 0.95f);
        }
    }
}
