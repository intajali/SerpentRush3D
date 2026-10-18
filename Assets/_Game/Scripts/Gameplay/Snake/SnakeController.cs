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

        [Tooltip("Optional override. When assigned, this action is used instead of Controls.")]
        [SerializeField] InputActionReference _steerAction;

        [Tooltip("Player/Steer on SnakeControls. WASD, arrows, and <Gamepad>/leftStick. Empty uses the same bindings in code.")]
        [SerializeField] InputActionAsset _controls;

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

            var action = ResolveAction();
            _input = new DeviceSteerInput(action);
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

        InputAction ResolveAction()
        {
            if (_steerAction != null && _steerAction.action != null)
                return _steerAction.action;

            if (_controls != null)
            {
                var found = _controls.FindAction("Player/Steer", false);
                if (found != null)
                    return found;
            }

            _ownedAction = SnakeSteerBindings.Create();
            return _ownedAction;
        }

        Transform ResolveView()
        {
            if (_viewCamera != null)
                return _viewCamera.transform;

            return Camera.main != null ? Camera.main.transform : null;
        }
    }
}
