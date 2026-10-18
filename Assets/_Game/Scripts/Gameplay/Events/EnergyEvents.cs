namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Fired whenever the current run's energy total changes. UI can subscribe on the session bus.
    /// This is not written to disk.
    /// </summary>
    public readonly struct EnergyChanged
    {
        public EnergyChanged(
            int totalEnergy,
            int amountGained,
            int growthProgress,
            int growthThreshold,
            int segmentsGained,
            EnergyType source)
        {
            TotalEnergy = totalEnergy;
            AmountGained = amountGained;
            GrowthProgress = growthProgress;
            GrowthThreshold = growthThreshold;
            SegmentsGained = segmentsGained;
            Source = source;
        }

        public int TotalEnergy { get; }

        public int AmountGained { get; }

        public int GrowthProgress { get; }

        public int GrowthThreshold { get; }

        public int SegmentsGained { get; }

        public EnergyType Source { get; }
    }
}
