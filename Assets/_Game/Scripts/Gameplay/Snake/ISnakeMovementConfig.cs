namespace SerpentRush.Gameplay
{
    public interface ISnakeMovementConfig
    {
        float ForwardSpeed { get; }

        float MaxTurnDegreesPerSecond { get; }

        float InputDeadzone { get; }

        bool CameraRelativeSteering { get; }
    }
}
