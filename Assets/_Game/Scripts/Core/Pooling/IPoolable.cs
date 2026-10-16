namespace SerpentRush.Core.Pooling
{
    /// <summary>
    /// Optional callbacks for a component that lives on a pooled GameObject.
    /// <see cref="GameObjectPool"/> calls these after activation and before deactivation.
    /// </summary>
    public interface IPoolable
    {
        void OnSpawned();

        void OnDespawned();
    }
}
