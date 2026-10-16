using SerpentRush.Core.Time;
using UnityEngine;

namespace SerpentRush.Managers
{
    /// <summary>
    /// Unity adapter for <see cref="ITimeControl"/>.
    /// </summary>
    public sealed class UnityTimeControl : ITimeControl
    {
        public float TimeScale
        {
            get => Time.timeScale;
            set => Time.timeScale = value;
        }
    }
}
