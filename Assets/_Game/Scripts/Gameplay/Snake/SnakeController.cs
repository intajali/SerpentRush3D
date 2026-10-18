using UnityEngine;
using UnityEngine.InputSystem;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Reads steer input and drives <see cref="SnakeMovement"/>. The head moves only while control is enabled.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SnakeMovement))]
    [AddComponentMenu("Serpent Rush/Snake Controller")]
    public sealed class SnakeController : MonoBehaviour
    {
        [SerializeField] SnakeMovement _movement;

        [Tooltip("Optional. When empty, WASD, arrow keys, and the left stick are used.")]
        [SerializeField] InputActionReference _steerAction;

        [Tooltip("Optional. Camera Relative Steering uses this view. Falls back to Camera.main.")]
        [SerializeField] Camera _viewCamera;

        [SerializeField] bool _controlEnabled = true;

        ISteerInput _input;
        InputAction _ownedAction;

        public bool ControlEnabled => _controlEnabled;

        public void SetControlEnabled(bool enabled)
        {
            _controlEnabled = enabled;
        }

        void Awake()
        {
            if (_movement == null)
                _movement = GetComponent<SnakeMovement>();

            if (_steerAction != null && _steerAction.action != null)
            {
                _input = new DeviceSteerInput(_steerAction.action);
                return;
            }

            _ownedAction = SnakeSteerBindings.Create();
            _input = new DeviceSteerInput(_ownedAction);
        }

        void OnEnable()
        {
            _input?.Enable();
        }

        void OnDisable()
        {
            _input?.Disable();
        }

        void OnDestroy()
        {
            _ownedAction?.Dispose();
            _ownedAction = null;
        }

        void Update()
        {
            if (!_controlEnabled || _movement == null || _input == null)
                return;

            var desired = SteerAim.ToWorld(_input.Read(), _movement.Config, ResolveView());
            _movement.Tick(desired, Time.deltaTime);
        }

        Transform ResolveView()
        {
            if (_viewCamera != null)
                return _viewCamera.transform;

            return Camera.main != null ? Camera.main.transform : null;
        }
    }
}
