using UnityEngine;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Point values, growth, and spawn tuning for energy orbs. Nothing here is saved between sessions.
    /// </summary>
    public interface IEnergyConfig
    {
        int CommonValue { get; }

        int RareValue { get; }

        int EpicValue { get; }

        int GrowthThreshold { get; }

        int ActiveCount { get; }

        float SpawnHalfExtent { get; }

        float SpawnHeight { get; }

        float MinimumSpacing { get; }

        float ClearanceFromSnake { get; }

        float FeedbackDuration { get; }

        int PoolCapacity { get; }

        int ValueOf(EnergyType type);

        float DiameterOf(EnergyType type);

        Color ColorOf(EnergyType type);

        float WeightOf(EnergyType type);
    }
}
