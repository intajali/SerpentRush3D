namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Read-only session settings. Systems depend on this rather than the asset type.
    /// </summary>
    public interface IGameConfig
    {
        int TargetFrameRate { get; }

        int VSyncCount { get; }

        bool LogStateTransitions { get; }

        bool StartInPlaying { get; }

        float ArenaHalfExtent { get; }

        int SegmentPoolCapacity { get; }

        int PickupPoolCapacity { get; }
    }
}
