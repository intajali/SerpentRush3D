using UnityEngine;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Chase camera in the style of Worms Clash: behind the head, looking ahead, yaw easing onto the turn.
    /// The head stays in the lower part of the frame and the path in front stays visible.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Serpent Rush/Snake Camera Follow")]
    public sealed class SnakeCameraFollow : MonoBehaviour
    {
        [SerializeField] Transform _target;

        [Tooltip("X is sideways, Y is height, Z is distance behind the head. Negative Z sits behind.")]
        [SerializeField] Vector3 _followOffset = new Vector3(0f, 12f, -8f);

        [Tooltip("How far ahead of the head the camera looks. This keeps the head low in the frame.")]
        [SerializeField, Min(0f)] float _lookAhead = 10f;

        [SerializeField, Min(0f)] float _positionSharpness = 10f;

        [SerializeField, Min(0f)] float _lookSharpness = 14f;

        [Tooltip("How quickly the camera swings behind a turn. Lower values let the head lead the turn.")]
        [SerializeField, Min(0f)] float _yawSharpness = 7f;

        [SerializeField] bool _orbitWithHeading = true;

        Vector3 _followForward = Vector3.forward;
        bool _hasFrame;

        public void SetTarget(Transform target)
        {
            _target = target;
            _hasFrame = false;
        }

        void LateUpdate()
        {
            if (_target == null)
            {
                var snake = FindAnyObjectByType<SnakeController>();
                if (snake != null)
                    _target = snake.transform;
            }

            if (_target == null)
                return;

            UpdateFollowForward();

            var desired = DesiredPosition();
            var lookTarget = _target.position + (_followForward * _lookAhead) + (Vector3.up * 0.4f);
            var toTarget = lookTarget - desired;
            if (toTarget.sqrMagnitude < 0.0001f)
                toTarget = _followForward;

            var desiredRotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            if (!_hasFrame)
            {
                transform.SetPositionAndRotation(desired, desiredRotation);
                _hasFrame = true;
                return;
            }

            transform.position = Vector3.Lerp(transform.position, desired, Blend(_positionSharpness));
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Blend(_lookSharpness));
        }

        Vector3 DesiredPosition()
        {
            if (!_orbitWithHeading)
                return _target.position + _followOffset;

            var right = Vector3.Cross(Vector3.up, _followForward);
            return _target.position
                + (right * _followOffset.x)
                + (Vector3.up * _followOffset.y)
                + (_followForward * _followOffset.z);
        }

        void UpdateFollowForward()
        {
            var facing = SteerAim.Planar(_target.forward);
            if (facing.sqrMagnitude < 0.0001f)
                facing = _followForward.sqrMagnitude > 0.0001f ? _followForward : Vector3.forward;
            else
                facing.Normalize();

            if (!_hasFrame || _yawSharpness <= 0f)
            {
                _followForward = facing;
                return;
            }

            var smoothed = Vector3.Slerp(_followForward, facing, Blend(_yawSharpness));
            smoothed.y = 0f;
            _followForward = smoothed.sqrMagnitude < 0.0001f ? facing : smoothed.normalized;
        }

        static float Blend(float sharpness)
        {
            if (sharpness <= 0f)
                return 1f;

            return 1f - Mathf.Exp(-sharpness * Time.deltaTime);
        }
    }
}
