namespace SerpentRush.Gameplay
{
    /// <summary>
    /// What the snake head ran into. Damage amounts live on <see cref="HealthConfig"/>, not on the category.
    /// </summary>
    public enum CollisionCategory
    {
        None = 0,
        Obstacle = 1,
        EnemyHead = 2,
        EnemyBody = 3,
        Hazard = 4,
        Collectable = 5
    }
}
