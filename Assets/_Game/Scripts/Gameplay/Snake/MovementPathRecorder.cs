using System.Collections.Generic;
using UnityEngine;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Stores the head's traveled polyline and samples it by arc length.
    /// A point behind the head is taken from that history, so followers stay on the curve the head already took.
    /// </summary>
    public sealed class MovementPathRecorder
    {
        readonly List<PathSample> _samples = new List<PathSample>(1024);

        Vector3 _liveTip;
        float _pending;
        float _sampleSpacing = 0.05f;
        bool _hasLiveTip;

        public float Length { get; private set; }

        public float SampleSpacing
        {
            get => _sampleSpacing;
            set => _sampleSpacing = Mathf.Max(0.01f, value);
        }

        /// <summary>
        /// History kept behind the head. Older points are dropped. Zero keeps the whole path.
        /// </summary>
        public float RetainDistance { get; set; }

        public void Seed(Vector3 headPosition, Vector3 forward, float length, float sampleSpacing)
        {
            SampleSpacing = sampleSpacing;
            _samples.Clear();
            _pending = 0f;
            _hasLiveTip = false;
            _liveTip = headPosition;

            forward = SteerAim.Planar(forward);
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.forward;
            else
                forward.Normalize();

            float spacing = SampleSpacing;
            int steps = Mathf.Max(1, Mathf.CeilToInt(Mathf.Max(length, spacing) / spacing));
            float covered = steps * spacing;

            if (_samples.Capacity < steps + 1)
                _samples.Capacity = steps + 1;

            for (int i = 0; i <= steps; i++)
            {
                float distance = i * spacing;
                float behind = covered - distance;
                _samples.Add(new PathSample(headPosition - forward * behind, distance));
            }

            Length = covered;
        }

        public void Advance(Vector3 headPosition)
        {
            if (_samples.Count == 0)
            {
                _samples.Add(new PathSample(headPosition, 0f));
                _liveTip = headPosition;
                Length = 0f;
                return;
            }

            var origin = _hasLiveTip ? _liveTip : _samples[_samples.Count - 1].Position;
            float step = Vector3.Distance(origin, headPosition);
            if (step <= 0.000001f)
                return;

            Length += step;
            _pending += step;
            _liveTip = headPosition;
            _hasLiveTip = true;

            if (_pending + 0.000001f >= SampleSpacing)
            {
                _samples.Add(new PathSample(headPosition, Length));
                _pending = 0f;
                _hasLiveTip = false;
            }

            Trim();
        }

        /// <summary>
        /// Extends the path backward along the tail tangent until the stored span covers <paramref name="requiredSpan"/>.
        /// </summary>
        public void EnsureSpan(float requiredSpan)
        {
            if (_samples.Count == 0 || requiredSpan <= 0f)
                return;

            float span = Length - _samples[0].Distance;
            if (span + 0.0001f >= requiredSpan)
                return;

            float spacing = Mathf.Max(0.01f, SampleSpacing);
            float deficit = requiredSpan - span;
            int steps = Mathf.CeilToInt(deficit / spacing);
            float added = steps * spacing;

            var tail = _samples[0];
            var backward = _samples.Count > 1 ? tail.Position - _samples[1].Position : Vector3.back;
            if (backward.sqrMagnitude < 0.000001f)
                backward = Vector3.back;
            else
                backward.Normalize();

            var prefix = new PathSample[steps];
            for (int i = 0; i < steps; i++)
            {
                float behind = added - (i * spacing);
                prefix[i] = new PathSample(tail.Position + backward * behind, tail.Distance - behind);
            }

            _samples.InsertRange(0, prefix);
        }

        public void SampleBehind(float distanceBehindHead, out Vector3 position, out Vector3 tangent)
        {
            if (_samples.Count == 0)
            {
                position = _liveTip;
                tangent = Vector3.forward;
                return;
            }

            float target = Length - Mathf.Max(0f, distanceBehindHead);
            float oldest = _samples[0].Distance;
            if (target < oldest)
                target = oldest;
            else if (target > Length)
                target = Length;

            int count = _samples.Count;
            bool extra = _hasLiveTip && _samples[count - 1].Distance < Length;
            int total = count + (extra ? 1 : 0);

            int lo = 0;
            int hi = total - 1;
            while (lo < hi)
            {
                int mid = (lo + hi) >> 1;
                if (Point(mid).Distance < target)
                    lo = mid + 1;
                else
                    hi = mid;
            }

            if (lo >= total)
                lo = total - 1;

            if (lo == 0)
            {
                position = _samples[0].Position;
                tangent = Tangent(0, total);
                return;
            }

            var previous = Point(lo - 1);
            var next = Point(lo);
            float span = next.Distance - previous.Distance;
            float t = span > 0.000001f ? Mathf.Clamp01((target - previous.Distance) / span) : 1f;
            position = Vector3.Lerp(previous.Position, next.Position, t);

            var delta = next.Position - previous.Position;
            tangent = delta.sqrMagnitude > 0.000001f ? delta.normalized : Tangent(lo - 1, total);
        }

        void Trim()
        {
            if (RetainDistance <= 0f || _samples.Count < 3)
                return;

            float cutoff = Length - RetainDistance;
            int remove = 0;
            while (remove < _samples.Count - 2 && _samples[remove + 1].Distance < cutoff)
                remove++;

            if (remove > 0)
                _samples.RemoveRange(0, remove);
        }

        PathSample Point(int index)
        {
            if (index < _samples.Count)
                return _samples[index];

            return new PathSample(_liveTip, Length);
        }

        Vector3 Tangent(int index, int total)
        {
            int next = Mathf.Min(index + 1, total - 1);
            if (next == index)
                return Vector3.forward;

            var delta = Point(next).Position - Point(index).Position;
            return delta.sqrMagnitude < 0.000001f ? Vector3.forward : delta.normalized;
        }

        readonly struct PathSample
        {
            public PathSample(Vector3 position, float distance)
            {
                Position = position;
                Distance = distance;
            }

            public Vector3 Position { get; }

            public float Distance { get; }
        }
    }
}
