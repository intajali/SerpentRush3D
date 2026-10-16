using System;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Applies session settings and signals readiness on the following tick.
    /// Later startup work belongs in this state, before the ready signal.
    /// </summary>
    public sealed class BootstrapState : GameState
    {
        readonly IGameConfig _config;
        readonly IRuntimeSettings _runtimeSettings;
        readonly Action _onReady;
        bool _readyRaised;

        public BootstrapState(IGameConfig config, IRuntimeSettings runtimeSettings, Action onReady)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _runtimeSettings = runtimeSettings ?? throw new ArgumentNullException(nameof(runtimeSettings));
            _onReady = onReady ?? throw new ArgumentNullException(nameof(onReady));
        }

        public override GameStateId Id => GameStateId.Bootstrap;

        public override void Enter()
        {
            _readyRaised = false;
            _runtimeSettings.Apply(_config.VSyncCount, _config.TargetFrameRate);
        }

        public override void Tick(float deltaTime)
        {
            if (_readyRaised)
                return;

            _readyRaised = true;
            _onReady();
        }
    }

    public sealed class BootstrapStateBuilder : IGameStateBuilder
    {
        readonly IRuntimeSettings _runtimeSettings;

        public BootstrapStateBuilder(IRuntimeSettings runtimeSettings)
        {
            _runtimeSettings = runtimeSettings ?? throw new ArgumentNullException(nameof(runtimeSettings));
        }

        public GameStateId Id => GameStateId.Bootstrap;

        public GameState Create(GameStateContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return new BootstrapState(
                context.Config,
                _runtimeSettings,
                () => context.Queue.QueueTransition(GameStateId.MainMenu));
        }
    }
}
