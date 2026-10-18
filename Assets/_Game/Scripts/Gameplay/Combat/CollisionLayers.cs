using UnityEngine;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Layer names and the collision matrix for the snake. The head only tests obstacles, hazards, enemies, and collectables.
    /// </summary>
    public static class CollisionLayers
    {
        public const string PlayerHead = "PlayerHead";
        public const string PlayerBody = "PlayerBody";
        public const string Obstacle = "Obstacle";
        public const string Hazard = "Hazard";
        public const string EnemyHead = "EnemyHead";
        public const string EnemyBody = "EnemyBody";
        public const string Collectable = "Collectable";

        static bool _applied;

        public static void Apply()
        {
            if (_applied)
                return;

            Restrict(PlayerHead, Obstacle, Hazard, EnemyHead, EnemyBody, Collectable);
            Restrict(PlayerBody, Obstacle, Hazard, EnemyHead, EnemyBody);
            Restrict(Obstacle, PlayerHead, PlayerBody, EnemyHead);
            Restrict(Hazard, PlayerHead, PlayerBody, EnemyHead);
            Restrict(EnemyHead, "Default", PlayerHead, PlayerBody, Obstacle, Hazard, EnemyBody, Collectable);
            Restrict(EnemyBody, PlayerHead, PlayerBody, EnemyHead);
            Restrict(Collectable, PlayerHead, EnemyHead);
            _applied = true;
        }

        public static bool TryGetCategory(int layer, out CollisionCategory category)
        {
            if (layer == Named(Obstacle))
            {
                category = CollisionCategory.Obstacle;
                return true;
            }

            if (layer == Named(Hazard))
            {
                category = CollisionCategory.Hazard;
                return true;
            }

            if (layer == Named(EnemyHead))
            {
                category = CollisionCategory.EnemyHead;
                return true;
            }

            if (layer == Named(EnemyBody))
            {
                category = CollisionCategory.EnemyBody;
                return true;
            }

            if (layer == Named(Collectable))
            {
                category = CollisionCategory.Collectable;
                return true;
            }

            category = CollisionCategory.None;
            return false;
        }

        static void Restrict(string layerName, params string[] allowedNames)
        {
            int layer = Named(layerName);
            if (layer < 0)
                return;

            var allowed = new bool[32];
            for (int i = 0; i < allowedNames.Length; i++)
            {
                int other = Named(allowedNames[i]);
                if (other >= 0)
                    allowed[other] = true;
            }

            for (int other = 0; other < 32; other++)
                Physics.IgnoreLayerCollision(layer, other, !allowed[other]);
        }

        static int Named(string layerName)
        {
            return LayerMask.NameToLayer(layerName);
        }
    }
}
