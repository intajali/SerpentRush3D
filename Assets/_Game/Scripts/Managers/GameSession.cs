using System;
using System.Collections.Generic;
using SerpentRush.Core.Events;
using SerpentRush.Core.StateMachine;
using SerpentRush.Core.Time;
using SerpentRush.Gameplay;
using UnityEngine;

namespace SerpentRush.Managers
{
    /// <summary>
    /// Session flow without a Unity lifecycle. Dependencies are constructor-injected.
    /// </summary>
    public sealed class GameSession : ISessionQueue, IDisposable
    {
        readonly IGameConfig _config;
        readonly IEventBus _bus;
        readonly IStateMachine<GameState> _machine;
        readonly IGameFlowPolicy _policy;
        readonly ITimeControl _time;
        readonly Dictionary<GameStateId, GameState> _states = new Dictionary<GameStateId, GameState>();
        readonly Queue<FlowStep> _queue = new Queue<FlowStep>();

        GameStateId _currentStateId;
        bool _subscribed;
        bool _bootstrapEntered;

        public GameSession(
            IGameConfig config,
            IEventBus bus,
            IStateMachine<GameState> machine,
            IGameFlowPolicy policy,
            ITimeControl time,
            IGameStateFactory factory)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _machine = machine ?? throw new ArgumentNullException(nameof(machine));
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
            _time = time ?? throw new ArgumentNullException(nameof(time));

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            var context = new GameStateContext(_config, this);
            foreach (GameStateId id in Enum.GetValues(typeof(GameStateId)))
            {
                if (id == GameStateId.None)
                    continue;

                _states.Add(id, factory.Create(id, context));
            }

            Subscribe();
        }

        public GameStateId CurrentStateId => _currentStateId;

        public void Tick(float unscaledDeltaTime)
        {
            if (!_bootstrapEntered)
            {
                _bootstrapEntered = true;
                TryTransition(GameStateId.Bootstrap, string.Empty);
            }

            _machine.Tick(unscaledDeltaTime);
            DrainQueue();
        }

        public void QueueTransition(GameStateId id, string reason = null)
        {
            _queue.Enqueue(new FlowStep(id, reason));
        }

        public void Dispose()
        {
            Unsubscribe();
            _machine.Stop();
            _time.TimeScale = 1f;
        }

        void Subscribe()
        {
            if (_subscribed)
                return;

            _bus.Subscribe<PlayRequested>(OnPlayRequested);
            _bus.Subscribe<PauseRequested>(OnPauseRequested);
            _bus.Subscribe<ResumeRequested>(OnResumeRequested);
            _bus.Subscribe<GameOverRequested>(OnGameOverRequested);
            _bus.Subscribe<MainMenuRequested>(OnMainMenuRequested);
            _subscribed = true;
        }

        void Unsubscribe()
        {
            if (!_subscribed)
                return;

            _bus.Unsubscribe<PlayRequested>(OnPlayRequested);
            _bus.Unsubscribe<PauseRequested>(OnPauseRequested);
            _bus.Unsubscribe<ResumeRequested>(OnResumeRequested);
            _bus.Unsubscribe<GameOverRequested>(OnGameOverRequested);
            _bus.Unsubscribe<MainMenuRequested>(OnMainMenuRequested);
            _subscribed = false;
        }

        void OnPlayRequested(PlayRequested payload)
        {
            QueueTransition(GameStateId.Playing);
        }

        void OnPauseRequested(PauseRequested payload)
        {
            QueueTransition(GameStateId.Paused);
        }

        void OnResumeRequested(ResumeRequested payload)
        {
            QueueTransition(GameStateId.Playing);
        }

        void OnMainMenuRequested(MainMenuRequested payload)
        {
            QueueTransition(GameStateId.MainMenu);
        }

        void OnGameOverRequested(GameOverRequested payload)
        {
            QueueTransition(GameStateId.GameOver, payload.Reason);
        }

        void DrainQueue()
        {
            var pending = _queue.Count;
            for (int i = 0; i < pending; i++)
            {
                var step = _queue.Dequeue();
                TryTransition(step.Id, step.Reason);
            }
        }

        void TryTransition(GameStateId next, string reason)
        {
            if (!_policy.CanTransition(_currentStateId, next))
            {
                if (_config.LogStateTransitions)
                    Debug.LogWarning($"[SerpentRush] Ignored transition {_currentStateId} -> {next}.");

                return;
            }

            if (_states[next] is IGameOverDetails gameOver)
                gameOver.SetReason(reason);

            var previous = _currentStateId;
            _machine.ChangeState(_states[next]);
            _currentStateId = next;
            _time.TimeScale = _policy.TimeScaleFor(next);

            if (_config.LogStateTransitions)
            {
                var suffix = next == GameStateId.GameOver && !string.IsNullOrEmpty(reason)
                    ? $" ({reason})"
                    : string.Empty;
                Debug.Log($"[SerpentRush] {previous} -> {next}{suffix}");
            }

            _bus.Publish(new GameStateChanged(previous, next));
        }

        readonly struct FlowStep
        {
            public FlowStep(GameStateId id, string reason)
            {
                Id = id;
                Reason = reason ?? string.Empty;
            }

            public GameStateId Id { get; }

            public string Reason { get; }
        }
    }
}
