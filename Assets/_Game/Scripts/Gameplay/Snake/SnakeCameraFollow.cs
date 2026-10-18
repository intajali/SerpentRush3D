using System.Collections.Generic;
using UnityEngine;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Stays behind the snake, but waits before swinging around a turn so the head rotates first.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Serpent Rush/Snake Camera Follow")]
    public sealed class SnakeCameraFollow : MonoBehaviour
    {
        [SerializeField] Transform _target;

        [Tooltip("X is sideways, Y is height, Z is distance behind the head.")]
        [SerializeField] Vector3 _followOffset = new Vector3(0f, 8f, -10f);

        [SerializeField, Min(0f)] float _positionSharpness = 4f;

        [SerializeField, Min(0f)] float _lookSharpness = 8f;

        [Tooltip("How long the camera keeps the previous heading after the snake turns.")]
        [SerializeField, Min(0f)] float _headingDelay = 0.45f;

        [SerializeField] bool _orbitWithHeading = true;

        readonly List<HeadingSample> _headings = new List<HeadingSample>(128);
        bool _hasFrame;

        public void SetTarget(Transform target)
        {
            _target = target;
            _hasFrame = false;
            _headings.Clear();
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

            RecordHeading();
            var desired = DesiredPosition();
            var lookTarget = _target.position + Vector3.up * 0.5f;
            var toTarget = lookTarget - desired;
            if (toTarget.sqrMagnitude < 0.0001f)
                toTarget = Vector3.forward;

            var desiredRotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            if (!_hasFrame)
            {
                transform.SetPositionAndRotation(desired, desiredRotation);
                _hasFrame = true;
                return;
            }

            var positionBlend = Blend(_positionSharpness);
            var lookBlend = Blend(_lookSharpness);
            transform.position = Vector3.Lerp(transform.position, desired, positionBlend);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, lookBlend);
        }

        Vector3 DesiredPosition()
        {
            if (!_orbitWithHeading)
                return _target.position + _followOffset;

            var forward = DelayedHeading();

            var right = Vector3.Cross(Vector3.up, forward);
            return _target.position
                + right * _followOffset.x
                + Vector3.up * _followOffset.y
                + forward * _followOffset.z;
        }

        void RecordHeading()
        {
            var forward = SteerAim.Planar(_target.forward);
            if (forward.sqrMagnitude < 0.0001f)
                forward = _headings.Count > 0 ? _headings[_headings.Count - 1].Forward : Vector3.forward;
            else
                forward.Normalize();

            float now = Time.time;
            if (_headings.Count == 0 || now > _headings[_headings.Count - 1].Time)
                _headings.Add(new HeadingSample(now, forward));

            float keepAfter = now - _headingDelay - 0.5f;
            int remove = 0;
            while (remove < _headings.Count - 2 && _headings[remove + 1].Time < keepAfter)
                remove++;

            if (remove > 0)
                _headings.RemoveRange(0, remove);
        }

        Vector3 DelayedHeading()
        {
            if (_headings.Count == 0)
                return Vector3.forward;

            if (_headingDelay <= 0f)
                return _headings[_headings.Count - 1].Forward;

            float sampleTime = Time.time - _headingDelay;
            if (sampleTime <= _headings[0].Time)
                return _headings[0].Forward;

            for (int i = 1; i < _headings.Count; i++)
            {
                if (_headings[i].Time < sampleTime)
                    continue;

                var previous = _headings[i - 1];
                var next = _headings[i];
                float span = next.Time - previous.Time;
                float t = span > 0.0001f ? Mathf.Clamp01((sampleTime - previous.Time) / span) : 1f;
                var forward = Vector3.Slerp(previous.Forward, next.Forward, t);
                forward.y = 0f;
                return forward.sqrMagnitude < 0.0001f ? next.Forward : forward.normalized;
            }

            return _headings[_headings.Count - 1].Forward;
        }

        static float Blend(float sharpness)
        {
            if (sharpness <= 0f)
                return 1f;

            return 1f - Mathf.Exp(-sharpness * Time.deltaTime);
        }

        readonly struct HeadingSample
        {
            public HeadingSample(float time, Vector3 forward)
            {
                Time = time;
                Forward = forward;
            }

            public float Time { get; }

            public Vector3 Forward { get; }
        }
    }
}
