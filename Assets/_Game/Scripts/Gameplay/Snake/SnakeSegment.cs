using SerpentRush.Core.Pooling;
using UnityEngine;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// One pooled body piece. It has no Rigidbody; the body controller writes its pose.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Serpent Rush/Snake Segment")]
    public sealed class SnakeSegment : MonoBehaviour, IPoolable
    {
        [SerializeField] Transform _visual;

        public bool HasPose { get; private set; }

        /// <summary>
        /// Sizes the mesh child. The capsule's length axis is local Y, already turned onto the segment's forward.
        /// </summary>
        public void SetSize(float diameter, float length)
        {
            if (_visual == null && transform.childCount > 0)
                _visual = transform.GetChild(0);

            if (_visual == null)
                return;

            _visual.localScale = new Vector3(diameter, Mathf.Max(0.01f, length) * 0.5f, diameter);
        }

        public void Apply(Vector3 position, Quaternion rotation, float rotationBlend)
        {
            if (!HasPose || rotationBlend >= 1f)
            {
                transform.SetPositionAndRotation(position, rotation);
                HasPose = true;
                return;
            }

            transform.position = position;
            if (rotationBlend > 0f)
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationBlend);
        }

        public void OnSpawned()
        {
        }

        public void OnDespawned()
        {
            HasPose = false;
        }
    }
}
