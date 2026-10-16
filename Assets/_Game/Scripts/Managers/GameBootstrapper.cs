using SerpentRush.Core.Events;
using SerpentRush.Core.StateMachine;
using SerpentRush.Gameplay;
using UnityEngine;

namespace SerpentRush.Managers
{
    /// <summary>
    /// Composition root. Builds the session graph and hands it to <see cref="GameManager"/>.
    /// This is the only place that chooses the concrete implementations.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GameManager))]
    [DefaultExecutionOrder(-1000)]
    [AddComponentMenu("Serpent Rush/Game Bootstrapper")]
    public sealed class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] GameConfig _config;

        void Awake()
        {
            var manager = GetComponent<GameManager>();
            if (!manager.TryClaim())
            {
                Destroy(gameObject);
                return;
            }

            if (_config == null)
            {
                Debug.LogError("[SerpentRush] GameConfig is missing. Assign it on GameBootstrapper.");
                return;
            }

            var runtimeSettings = new UnityRuntimeSettings();
            IGameStateBuilder[] builders =
            {
                new BootstrapStateBuilder(runtimeSettings),
                new MainMenuStateBuilder(),
                new PlayingStateBuilder(),
                new PausedStateBuilder(),
                new GameOverStateBuilder()
            };

            manager.Initialize(
                _config,
                new EventBus(),
                new StateMachine<GameState>(),
                new GameFlowRules(),
                new UnityTimeControl(),
                new GameStateFactory(builders));
        }
    }
}
