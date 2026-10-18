using System;
using SerpentRush.Core.Pooling;
using UnityEngine;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// A pooled energy orb. The head's kinematic body trips its trigger.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Serpent Rush/Energy Collectable")]
    public sealed class EnergyCollectable : MonoBehaviour, IPoolable
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] Renderer _renderer;
        [SerializeField] SphereCollider _trigger;

        MaterialPropertyBlock _block;

        Action<EnergyCollectable> _onCollected;
        Action<EnergyCollectable> _onFeedbackFinished;
        Vector3 _basePosition;
        Vector3 _baseScale;
        Color _color;
        float _feedbackDuration;
        float _feedbackTimer;
        float _bobSeed;
        bool _collected;
        bool _playingFeedback;

        public EnergyType Type { get; private set; }

        public int Value { get; private set; }

        public void Prepare(
            EnergyType type,
            int value,
            float diameter,
            Color color,
            float feedbackDuration,
            Vector3 position,
            Action<EnergyCollectable> onCollected,
            Action<EnergyCollectable> onFeedbackFinished)
        {
            Type = type;
            Value = value;
            _color = color;
            _feedbackDuration = Mathf.Max(0f, feedbackDuration);
            _onCollected = onCollected;
            _onFeedbackFinished = onFeedbackFinished;
            _collected = false;
            _playingFeedback = false;
            _feedbackTimer = 0f;
            _bobSeed = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            _basePosition = position;
            _baseScale = Vector3.one * Mathf.Max(0.05f, diameter);

            transform.SetPositionAndRotation(_basePosition, Quaternion.identity);
            transform.localScale = _baseScale;

            if (_trigger != null)
            {
                _trigger.enabled = true;
                _trigger.isTrigger = true;
                _trigger.radius = 0.7f;
            }

            ApplyColor(_color);
        }

        public void OnSpawned()
        {
            _collected = false;
            _playingFeedback = false;
        }

        public void OnDespawned()
        {
            _collected = false;
            _playingFeedback = false;
            _onCollected = null;
            _onFeedbackFinished = null;
            if (_trigger != null)
                _trigger.enabled = false;
        }

        void Awake()
        {
            if (_renderer == null)
                _renderer = GetComponentInChildren<Renderer>();

            if (_trigger == null)
                _trigger = GetComponent<SphereCollider>();
        }

        void Update()
        {
            if (_playingFeedback)
            {
                AdvanceFeedback();
                return;
            }

            if (_collected)
                return;

            var bob = Mathf.Sin((Time.time * 2.2f) + _bobSeed) * 0.08f;
            var position = _basePosition;
            position.y += bob;
            transform.position = position;
        }

        void OnTriggerEnter(Collider other)
        {
            if (_collected || other == null)
                return;

            if (other.GetComponentInParent<SnakeController>() == null)
                return;

            _collected = true;
            if (_trigger != null)
                _trigger.enabled = false;

            var collected = _onCollected;
            _onCollected = null;
            collected?.Invoke(this);
        }

        public void PlayFeedback()
        {
            if (_feedbackDuration <= 0f)
            {
                FinishFeedback();
                return;
            }

            _playingFeedback = true;
            _feedbackTimer = _feedbackDuration;
        }

        void AdvanceFeedback()
        {
            _feedbackTimer -= Time.deltaTime;
            float duration = Mathf.Max(0.0001f, _feedbackDuration);
            float u = 1f - Mathf.Clamp01(_feedbackTimer / duration);
            float punch = u < 0.3f
                ? Mathf.Lerp(1f, 1.5f, u / 0.3f)
                : Mathf.Lerp(1.5f, 0f, (u - 0.3f) / 0.7f);

            transform.localScale = _baseScale * punch;
            ApplyColor(Color.Lerp(_color, Color.white, Mathf.Clamp01(u * 1.4f)));

            if (_feedbackTimer > 0f)
                return;

            FinishFeedback();
        }

        void FinishFeedback()
        {
            _playingFeedback = false;
            var finished = _onFeedbackFinished;
            _onFeedbackFinished = null;
            finished?.Invoke(this);
        }

        void ApplyColor(Color color)
        {
            if (_renderer == null)
                return;

            if (_block == null)
                _block = new MaterialPropertyBlock();

            _renderer.GetPropertyBlock(_block);
            _block.SetColor(BaseColorId, color);
            _block.SetColor(ColorId, color);
            _renderer.SetPropertyBlock(_block);
        }
    }
}
