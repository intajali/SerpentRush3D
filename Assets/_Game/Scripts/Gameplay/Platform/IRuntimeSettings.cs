namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Applies engine settings during bootstrap. The Unity adapter lives outside Gameplay.
    /// </summary>
    public interface IRuntimeSettings
    {
        void Apply(int vSyncCount, int targetFrameRate);
    }
}
