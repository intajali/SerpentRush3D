namespace SerpentRush.Core.Combat
{
    /// <summary>
    /// Something that can lose and regain health. Callers do not need to know how the owner is built.
    /// </summary>
    public interface IDamageable
    {
        int CurrentHealth { get; }

        int MaxHealth { get; }

        bool IsImmune { get; }

        void ApplyDamage(int amount);

        void Heal(int amount);
    }
}
