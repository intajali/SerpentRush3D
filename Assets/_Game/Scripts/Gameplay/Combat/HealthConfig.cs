using UnityEngine;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Core health and how much each collision category hurts. Nothing here kills the snake in one hit by default.
    /// </summary>
    [CreateAssetMenu(fileName = "HealthConfig", menuName = "Serpent Rush/Health Config")]
    public sealed class HealthConfig : ScriptableObject
    {
        [SerializeField, Min(1)] int _maxHealth = 5;

        [Tooltip("Seconds after a damaging hit during which further hits are ignored.")]
        [SerializeField, Min(0f)] float _immunitySeconds = 0.75f;

        [SerializeField, Min(0f)] float _flashSeconds = 0.18f;

        [SerializeField] Color _damageColor = new Color(1f, 0.28f, 0.22f, 1f);

        [Header("Obstacle")]
        [SerializeField, Min(0)] int _obstacleDamage = 1;
        [SerializeField, Min(0)] int _obstacleSegments = 0;

        [Header("Enemy Head")]
        [SerializeField, Min(0)] int _enemyHeadDamage = 1;
        [SerializeField, Min(0)] int _enemyHeadSegments = 1;

        [Header("Enemy Body")]
        [Tooltip("Core damage when the snake still has body segments.")]
        [SerializeField, Min(0)] int _enemyBodyDamage = 0;
        [SerializeField, Min(0)] int _enemyBodySegments = 1;
        [Tooltip("Core damage when the body is already gone, so a tail hit can still reach the head.")]
        [SerializeField, Min(0)] int _enemyBodyDamageWithoutSegments = 1;

        [Header("Hazard")]
        [SerializeField, Min(0)] int _hazardDamage = 1;
        [SerializeField, Min(0)] int _hazardSegments = 0;

        [Header("Collectable")]
        [SerializeField, Min(0)] int _collectableDamage = 0;
        [SerializeField, Min(0)] int _collectableSegments = 0;

        public int MaxHealth => _maxHealth;

        public float ImmunitySeconds => _immunitySeconds;

        public float FlashSeconds => _flashSeconds;

        public Color DamageColor => _damageColor;

        public void GetResponse(CollisionCategory category, out int healthDamage, out int segments, out int healthDamageWithoutSegments)
        {
            switch (category)
            {
                case CollisionCategory.Obstacle:
                    healthDamage = _obstacleDamage;
                    segments = _obstacleSegments;
                    healthDamageWithoutSegments = 0;
                    return;
                case CollisionCategory.EnemyHead:
                    healthDamage = _enemyHeadDamage;
                    segments = _enemyHeadSegments;
                    healthDamageWithoutSegments = 0;
                    return;
                case CollisionCategory.EnemyBody:
                    healthDamage = _enemyBodyDamage;
                    segments = _enemyBodySegments;
                    healthDamageWithoutSegments = _enemyBodyDamageWithoutSegments;
                    return;
                case CollisionCategory.Hazard:
                    healthDamage = _hazardDamage;
                    segments = _hazardSegments;
                    healthDamageWithoutSegments = 0;
                    return;
                case CollisionCategory.Collectable:
                    healthDamage = _collectableDamage;
                    segments = _collectableSegments;
                    healthDamageWithoutSegments = 0;
                    return;
                default:
                    healthDamage = 0;
                    segments = 0;
                    healthDamageWithoutSegments = 0;
                    return;
            }
        }
    }
}
