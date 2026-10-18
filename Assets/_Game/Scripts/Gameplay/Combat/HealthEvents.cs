namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Fired when the snake's core health changes. UI subscribes on the session bus.
    /// Delta is negative for damage and positive for healing. This is not saved.
    /// </summary>
    public readonly struct HealthChanged
    {
        public HealthChanged(int current, int max, int delta, int segmentsLost, CollisionCategory source)
        {
            Current = current;
            Max = max;
            Delta = delta;
            SegmentsLost = segmentsLost;
            Source = source;
        }

        public int Current { get; }

        public int Max { get; }

        public int Delta { get; }

        public int SegmentsLost { get; }

        public CollisionCategory Source { get; }
    }
}
