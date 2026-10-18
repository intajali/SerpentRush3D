using UnityEngine;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Leaves the spot where the snake hit it and appears somewhere else on the ground plane.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Serpent Rush/Arena Obstacle")]
    public sealed class ArenaObstacle : MonoBehaviour
    {
        [Tooltip("Half the ground size, with a margin so the block stays on the plane. The Bootstrap plane is 100 by 100, so 46 keeps it inside.")]
        [SerializeField, Min(1f)] float _planeHalfExtent = 46f;

        [Tooltip("The new spot stays at least this far from the snake.")]
        [SerializeField, Min(0f)] float _clearanceFromSnake = 8f;

        [Tooltip("The new spot stays at least this far from the spot that was just hit.")]
        [SerializeField, Min(0f)] float _minimumMove = 12f;

        public void Relocate(Vector3 snakePosition)
        {
            float height = transform.position.y;
            var previous = transform.position;
            float clearance = _clearanceFromSnake * _clearanceFromSnake;
            float minimum = _minimumMove * _minimumMove;
            float extent = _planeHalfExtent;

            for (int attempt = 0; attempt < 24; attempt++)
            {
                var next = new Vector3(
                    Random.Range(-extent, extent),
                    height,
                    Random.Range(-extent, extent));

                if (TooClose(next, snakePosition, clearance))
                    continue;

                if (TooClose(next, previous, minimum))
                    continue;

                transform.position = next;
                return;
            }

            transform.position = new Vector3(
                Random.Range(-extent, extent),
                height,
                Random.Range(-extent, extent));
        }

        static bool TooClose(Vector3 a, Vector3 b, float minimumSqr)
        {
            var gap = a - b;
            gap.y = 0f;
            return gap.sqrMagnitude < minimumSqr;
        }
    }
}
