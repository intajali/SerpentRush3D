using System.Collections.Generic;
using UnityEngine;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Reads physics contacts on the snake head and forwards them to <see cref="SnakeHealth"/>.
    /// The head is moved by writing its position, so overlaps are queried after movement.
    /// Collectables are ignored here so pickup does not count as damage.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SnakeHealth))]
    [AddComponentMenu("Serpent Rush/Snake Hit Sensor")]
    public sealed class SnakeHitSensor : MonoBehaviour
    {
        [SerializeField] SnakeHealth _health;

        readonly Collider[] _found = new Collider[16];
        readonly List<int> _touching = new List<int>(8);
        readonly List<int> _touchedLastFrame = new List<int>(8);

        Collider _headCollider;
        int _damageMask;

        void Awake()
        {
            if (_health == null)
                _health = GetComponent<SnakeHealth>();

            _headCollider = GetComponentInChildren<Collider>();
            CollisionLayers.Apply();
            _damageMask = LayerMask.GetMask(
                CollisionLayers.Obstacle,
                CollisionLayers.Hazard,
                CollisionLayers.EnemyHead,
                CollisionLayers.EnemyBody);
        }

        void LateUpdate()
        {
            ScanOverlaps();
        }

        void ScanOverlaps()
        {
            if (_health == null || _headCollider == null || _damageMask == 0)
                return;

            Physics.SyncTransforms();
            var bounds = _headCollider.bounds;
            int count = Physics.OverlapBoxNonAlloc(
                bounds.center,
                bounds.extents,
                _found,
                Quaternion.identity,
                _damageMask,
                QueryTriggerInteraction.Collide);

            _touching.Clear();
            for (int i = 0; i < count; i++)
            {
                var other = _found[i];
                if (other == null || other == _headCollider)
                    continue;

                if (other.transform.IsChildOf(transform))
                    continue;

                int id = other.GetInstanceID();
                _touching.Add(id);
                bool alreadyTouching = false;
                for (int n = 0; n < _touchedLastFrame.Count; n++)
                {
                    if (_touchedLastFrame[n] != id)
                        continue;

                    alreadyTouching = true;
                    break;
                }

                TryHit(other, alreadyTouching);
            }

            _touchedLastFrame.Clear();
            for (int i = 0; i < _touching.Count; i++)
                _touchedLastFrame.Add(_touching[i]);
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision != null)
                TryHit(collision.collider, false);
        }

        void OnTriggerEnter(Collider other)
        {
            TryHit(other, false);
        }

        void OnCollisionStay(Collision collision)
        {
            if (collision != null)
                TryHit(collision.collider, true);
        }

        void OnTriggerStay(Collider other)
        {
            TryHit(other, true);
        }

        void TryHit(Collider other, bool staying)
        {
            if (_health == null || other == null)
                return;

            if (other.GetComponentInParent<SnakeHealth>() == _health)
                return;

            if (!CollisionLayers.TryGetCategory(other.gameObject.layer, out var category))
                return;

            if (category == CollisionCategory.Collectable)
                return;

            if (staying && category != CollisionCategory.Hazard)
                return;

            _health.ReceiveHit(category);
            if (staying || category != CollisionCategory.Obstacle)
                return;

            var obstacle = other.GetComponentInParent<ArenaObstacle>();
            if (obstacle != null)
                obstacle.Relocate(transform.position);
        }
    }
}
