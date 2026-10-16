using SerpentRush.Gameplay;
using UnityEngine;

namespace SerpentRush.Managers
{
    /// <summary>
    /// Unity adapter for frame pacing. VSync takes priority over the target frame rate.
    /// </summary>
    public sealed class UnityRuntimeSettings : IRuntimeSettings
    {
        public void Apply(int vSyncCount, int targetFrameRate)
        {
            QualitySettings.vSyncCount = vSyncCount;
            Application.targetFrameRate = vSyncCount > 0 ? -1 : targetFrameRate;
        }
    }
}
