using SerpentRush.Core.Events;
using SerpentRush.Gameplay;
using UnityEngine;

namespace SerpentRush.Managers
{
    /// <summary>
    /// Enables snake steering only while the session is in Playing.
    /// Put this on the same object as <see cref="GameManager"/> and assign the head.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GameManager))]
    [AddComponentMenu("Serpent Rush/Snake Session Link")]
    public sealed class SnakeSessionLink : MonoBehaviour
    {
        [SerializeField] SnakeController _snake;

        IEventBus _events;
        SnakeGrowthController _growth;
        bool _subscribed;

        void Start()
        {
            if (_snake == null)
            {
                Debug.LogError("[SerpentRush] SnakeSessionLink has no SnakeController assigned.", this);
                return;
            }

            var manager = GetComponent<GameManager>();
            _events = manager.Events;
            if (_events == null)
            {
                Debug.LogError("[SerpentRush] SnakeSessionLink could not read the session event bus.", this);
                return;
            }

            _growth = _snake.GetComponent<SnakeGrowthController>();
            _growth?.Bind(_events);

            _events.Subscribe<GameStateChanged>(OnStateChanged);
            _subscribed = true;
            Apply(manager.CurrentStateId);
        }

        void OnDestroy()
        {
            if (!_subscribed || _events == null)
                return;

            _events.Unsubscribe<GameStateChanged>(OnStateChanged);
            _subscribed = false;
        }

        void OnStateChanged(GameStateChanged change)
        {
            if (change.Current == GameStateId.Playing && change.Previous != GameStateId.Paused)
                _growth?.ResetRun();

            Apply(change.Current);
        }

        void Apply(GameStateId state)
        {
            _snake.SetControlEnabled(state == GameStateId.Playing);
        }
    }
}
