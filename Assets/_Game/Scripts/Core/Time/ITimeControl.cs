namespace SerpentRush.Core.Time
{
    /// <summary>
    /// Port over scaled time. Gameplay code uses this instead of <c>Time.timeScale</c>.
    /// </summary>
    public interface ITimeControl
    {
        float TimeScale { get; set; }
    }
}
