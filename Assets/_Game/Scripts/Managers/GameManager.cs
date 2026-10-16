using System;
using SerpentRush.Core.Events;
using SerpentRush.Core.StateMachine;
using SerpentRush.Core.Time;
using SerpentRush.Gameplay;
using UnityEngine;

namespace SerpentRush.Managers
{
    /// <summary>
    /// Unity adapter for <see cref="GameSession"/>. Scene components call the Request methods or publish on <see cref="Events"/>.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-500)]
    [AddComponentMenu("Serpent Rush/Game Manager")]
    public sealed class GameManager : MonoBehaviour
    {
        static GameManager _active;

        GameSession _session;
        IEventBus _events;

        public IEventBus Events => _events;

        public GameStateId CurrentStateId => _session != null ? _session.CurrentStateId : GameStateId.None;

        public bool IsInitialized => _session != null;

        public bool TryClaim()
        {
            if (_active != null && _active != this)
                return false;

            _active = this;
            return true;
        }

        public void Initialize(
            IGameConfig config,
            IEventBus events,
            IStateMachine<GameState> machine,
            IGameFlowPolicy policy,
            ITimeControl time,
            IGameStateFactory factory)
        {
            if (_active != this)
                return;

            if (_session != null)
                return;

            if (config == null)
            {
                Debug.LogError("[SerpentRush] GameConfig is missing. Assign it on GameBootstrapper.");
                return;
            }

            if (events == null)
                throw new ArgumentNullException(nameof(events));
            if (machine == null)
                throw new ArgumentNullException(nameof(machine));
            if (policy == null)
                throw new ArgumentNullException(nameof(policy));
            if (time == null)
                throw new ArgumentNullException(nameof(time));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            if (transform.parent != null)
                transform.SetParent(null);

            DontDestroyOnLoad(gameObject);
            _events = events;
            _session = new GameSession(config, events, machine, policy, time, factory);
        }

        void Awake()
        {
            if (!TryClaim())
                Destroy(gameObject);
        }

        void Start()
        {
            if (_active != this || IsInitialized)
                return;

            Debug.LogError("[SerpentRush] GameManager was not initialized. Add GameBootstrapper and assign a GameConfig asset.");
        }

        void Update()
        {
            if (_active != this || _session == null)
                return;

            _session.Tick(Time.unscaledDeltaTime);
        }

        void OnDestroy()
        {
            if (_active != this)
                return;

            _session?.Dispose();
            _session = null;
            _events = null;
            _active = null;
        }

        [ContextMenu("Request Play")]
        public void RequestPlay()
        {
            Publish(new PlayRequested());
        }

        [ContextMenu("Request Pause")]
        public void RequestPause()
        {
            Publish(new PauseRequested());
        }

        [ContextMenu("Request Resume")]
        public void RequestResume()
        {
            Publish(new ResumeRequested());
        }

        [ContextMenu("Request Main Menu")]
        public void RequestMainMenu()
        {
            Publish(new MainMenuRequested());
        }

        [ContextMenu("Request Game Over")]
        public void RequestGameOver()
        {
            RequestGameOver(string.Empty);
        }

        public void RequestGameOver(string reason)
        {
            Publish(new GameOverRequested(reason));
        }

        void Publish<T>(T payload) where T : struct
        {
            if (_events == null)
            {
                Debug.LogWarning("[SerpentRush] Ignored flow request because GameManager is not initialized.");
                return;
            }

            _events.Publish(payload);
        }
    }
}
