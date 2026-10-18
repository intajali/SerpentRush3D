namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Tuning for the body that trails the head along its recorded path.
    /// </summary>
    public interface ISnakeBodyConfig
    {
        /// <summary>Arc length, in world units, from one segment center to the next.</summary>
        float SegmentSpacing { get; }

        /// <summary>Thickness of a body segment. Kept below the spacing so neighbors meet end to end.</summary>
        float SegmentDiameter { get; }

        /// <summary>Segments spawned when the body starts. The head is not included.</summary>
        int InitialSegmentCount { get; }

        /// <summary>Smallest travel distance before another path point is stored.</summary>
        float PathSampleSpacing { get; }

        /// <summary>Inactive segments the pool may keep.</summary>
        int PoolCapacity { get; }
    }
}
