using Unity.Cinemachine;
using UnityEngine;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Third-person Cinemachine camera behind the snake head.
    /// The yaw proxy glides in position and trails the heading, so the view eases through turns.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-200)]
    [AddComponentMenu("Serpent Rush/Snake Cinemachine Rig")]
    public sealed class SnakeCinemachineRig : MonoBehaviour
    {
        [SerializeField] Transform _target;

        [Tooltip("Optional. Segment count drives the zoom. Taken from the target when empty.")]
        [SerializeField] SnakeBodyController _body;

        [Tooltip("Camera that receives the Cinemachine Brain. Falls back to Camera.main.")]
        [SerializeField] Camera _outputCamera;

        [Tooltip("Distance behind the head at the starting length.")]
        [SerializeField, Min(0.5f)] float _cameraDistance = 13f;

        [Tooltip("Height above the head at the starting length. Maps to Third Person Follow Vertical Arm Length.")]
        [SerializeField, Min(0f)] float _cameraHeight = 10f;

        [Tooltip("Distance when the body reaches Segments For Full Zoom.")]
        [SerializeField, Min(0.5f)] float _maxCameraDistance = 18f;

        [Tooltip("Height when the body reaches Segments For Full Zoom.")]
        [SerializeField, Min(0f)] float _maxCameraHeight = 14f;

        [Tooltip("Extra distance added as forward speed rises from the start value to the cap.")]
        [SerializeField, Min(0f)] float _speedDistanceBonus = 5f;

        [Tooltip("Extra height added as forward speed rises from the start value to the cap.")]
        [SerializeField, Min(0f)] float _speedHeightBonus = 2.5f;

        [SerializeField, Min(1)] int _segmentsForFullZoom = 40;

        [SerializeField, Min(0.01f)] float _zoomSmoothTime = 0.6f;

        [Tooltip("How quickly the camera swings behind a turn. Lower values let the head lead.")]
        [SerializeField, Min(0f)] float _yawSharpness = 3.2f;

        [Tooltip("Seconds for the follow point to catch the head. This is the glide. Cinemachine damping stays off so it is not filtered twice.")]
        [SerializeField, Min(0f)] float _positionSmoothTime = 0.12f;

        [Tooltip("Seconds for the aim to settle. Kept tiny so the look does not swim against the turn.")]
        [SerializeField] Vector2 _aimDamping = new Vector2(0.05f, 0.04f);

        [Tooltip("How far ahead of the follow point the camera looks, in the smoothed heading.")]
        [SerializeField, Min(0f)] float _lookAhead = 8f;

        [Tooltip("Where the aim point sits on screen. 0 is center. Positive tips the view down onto the ground ahead.")]
        [SerializeField, Range(-0.4f, 0.4f)] float _screenY = 0.06f;

        Transform _yaw;
        SnakeMovement _movement;
        CinemachineCamera _virtualCamera;
        CinemachineThirdPersonFollow _follow;
        CinemachineRotationComposer _aim;
        Vector3 _followVelocity;
        float _smoothedDistance;
        float _smoothedHeight;
        float _distanceVelocity;
        float _heightVelocity;
        bool _hasYaw;

        void Awake()
        {
            ResolveTargets();
            DisableLegacyFollow();
            _smoothedDistance = TargetDistance();
            _smoothedHeight = TargetHeight();
            BuildPipeline();
        }

        void LateUpdate()
        {
            ResolveTargets();
            if (_target == null || _yaw == null || _follow == null)
                return;

            TrackYaw();
            ApplyZoom();
            ApplyFraming();
        }

        void ResolveTargets()
        {
            if (_target == null)
            {
                var snake = FindAnyObjectByType<SnakeController>();
                if (snake != null)
                    _target = snake.transform;
            }

            if (_body == null && _target != null)
                _body = _target.GetComponent<SnakeBodyController>();

            if (_movement == null && _target != null)
                _movement = _target.GetComponent<SnakeMovement>();

            if (_outputCamera == null)
                _outputCamera = Camera.main;
        }

        void DisableLegacyFollow()
        {
            var follows = FindObjectsByType<SnakeCameraFollow>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < follows.Length; i++)
                follows[i].enabled = false;
        }

        void BuildPipeline()
        {
            if (_outputCamera == null)
            {
                Debug.LogError("[SerpentRush] Snake Cinemachine Rig needs a camera in the scene.");
                return;
            }

            var brain = _outputCamera.GetComponent<CinemachineBrain>();
            if (brain == null)
                brain = _outputCamera.gameObject.AddComponent<CinemachineBrain>();

            // The head moves in Update and also writes a kinematic Rigidbody.
            // SmartUpdate would sample that body in FixedUpdate and jitter against the transform.
            brain.UpdateMethod = CinemachineBrain.UpdateMethods.LateUpdate;
            brain.BlendUpdateMethod = CinemachineBrain.BrainUpdateMethods.LateUpdate;

            var cameraObject = new GameObject("Snake Third Person");
            cameraObject.transform.SetParent(transform, false);
            cameraObject.SetActive(false);

            _virtualCamera = cameraObject.AddComponent<CinemachineCamera>();
            _follow = cameraObject.AddComponent<CinemachineThirdPersonFollow>();
            _aim = cameraObject.AddComponent<CinemachineRotationComposer>();

            var lens = LensSettings.Default;
            lens.FieldOfView = _outputCamera.fieldOfView;
            lens.NearClipPlane = _outputCamera.nearClipPlane;
            lens.FarClipPlane = _outputCamera.farClipPlane;
            _virtualCamera.Lens = lens;

            _yaw = new GameObject("Camera Yaw").transform;
            _yaw.SetParent(transform, false);
            _virtualCamera.Follow = _yaw;
            _virtualCamera.LookAt = _yaw;

            _follow.CameraSide = 0.5f;
            _follow.ShoulderOffset = Vector3.zero;
            _follow.Damping = Vector3.zero;

            _aim.CenterOnActivate = true;
            _aim.Lookahead = new LookaheadSettings { Enabled = false };

            cameraObject.SetActive(true);
            ApplyZoom();
            ApplyFraming();
        }

        void TrackYaw()
        {
            if (!_hasYaw || _positionSmoothTime <= 0f)
            {
                _yaw.position = _target.position;
                _followVelocity = Vector3.zero;
            }
            else
            {
                _yaw.position = Vector3.SmoothDamp(
                    _yaw.position,
                    _target.position,
                    ref _followVelocity,
                    _positionSmoothTime);
            }

            var facing = SteerAim.Planar(_target.forward);
            if (facing.sqrMagnitude < 0.0001f)
                facing = _hasYaw ? _yaw.forward : Vector3.forward;
            facing.y = 0f;
            if (facing.sqrMagnitude < 0.0001f)
                return;
            facing.Normalize();

            var desired = Quaternion.LookRotation(facing, Vector3.up);
            if (!_hasYaw || _yawSharpness <= 0f)
            {
                _yaw.rotation = desired;
                _hasYaw = true;
                return;
            }

            var blend = 1f - Mathf.Exp(-_yawSharpness * Time.deltaTime);
            _yaw.rotation = Quaternion.Slerp(_yaw.rotation, desired, blend);
        }

        void ApplyZoom()
        {
            var distance = TargetDistance();
            var height = TargetHeight();
            if (_zoomSmoothTime <= 0f)
            {
                _smoothedDistance = distance;
                _smoothedHeight = height;
            }
            else
            {
                _smoothedDistance = Mathf.SmoothDamp(_smoothedDistance, distance, ref _distanceVelocity, _zoomSmoothTime);
                _smoothedHeight = Mathf.SmoothDamp(_smoothedHeight, height, ref _heightVelocity, _zoomSmoothTime);
            }

            _follow.CameraDistance = _smoothedDistance;
            _follow.VerticalArmLength = _smoothedHeight;
            _follow.Damping = Vector3.zero;
        }

        void ApplyFraming()
        {
            var composition = ScreenComposerSettings.Default;
            composition.ScreenPosition = new Vector2(0f, _screenY);
            _aim.Composition = composition;
            _aim.Damping = new Vector2(Mathf.Max(0f, _aimDamping.x), Mathf.Max(0f, _aimDamping.y));
            _aim.TargetOffset = new Vector3(0f, 0.4f, _lookAhead);
            _aim.Lookahead = new LookaheadSettings { Enabled = false };
        }

        float TargetDistance()
        {
            float distance = Mathf.Lerp(_cameraDistance, Mathf.Max(_cameraDistance, _maxCameraDistance), Zoom01());
            return distance + (_speedDistanceBonus * Speed01());
        }

        float TargetHeight()
        {
            float height = Mathf.Lerp(_cameraHeight, Mathf.Max(_cameraHeight, _maxCameraHeight), Zoom01());
            return height + (_speedHeightBonus * Speed01());
        }

        float Speed01()
        {
            if (_movement == null || _movement.Config == null)
                return 0f;

            float start = _movement.Config.ForwardSpeed;
            float limit = _movement.Config.MaxForwardSpeed;
            if (limit <= start + 0.01f)
                return 0f;

            return Mathf.Clamp01((_movement.CurrentSpeed - start) / (limit - start));
        }

        float Zoom01()
        {
            if (_body == null || _segmentsForFullZoom <= 0)
                return 0f;

            return Mathf.Clamp01(_body.SegmentCount / (float)_segmentsForFullZoom);
        }
    }
}
