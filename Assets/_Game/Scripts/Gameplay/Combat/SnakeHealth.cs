using SerpentRush.Core.Combat;
using SerpentRush.Core.Events;
using UnityEngine;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Core health for the snake head. Hits can shorten the body before they reach this health.
    /// At zero, the session is asked to enter game over.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SnakeBodyController))]
    [AddComponentMenu("Serpent Rush/Snake Health")]
    public sealed class SnakeHealth : MonoBehaviour, IDamageable
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] HealthConfig _config;
        [SerializeField] SnakeBodyController _body;
        [SerializeField] Renderer _renderer;

        IEventBus _bus;
        MaterialPropertyBlock _block;
        Color _baseColor = Color.white;
        float _immuneUntil;
        float _flashUntil;
        int _current = 1;
        bool _accepting = true;
        bool _defeated;
        bool _hasBaseColor;
        bool _reportedMissingConfig;

        public int CurrentHealth => _current;

        public int MaxHealth => _config != null ? Mathf.Max(1, _config.MaxHealth) : 1;

        public bool IsImmune => Time.time < _immuneUntil;

        public event System.Action<HealthChanged> Changed;

        void Awake()
        {
            if (_body == null)
                _body = GetComponent<SnakeBodyController>();

            if (_renderer == null)
                _renderer = GetComponentInChildren<Renderer>();

            CacheBaseColor();
            _current = MaxHealth;
        }

        void Update()
        {
            if (_flashUntil <= 0f)
                return;

            if (Time.unscaledTime >= _flashUntil)
            {
                _flashUntil = 0f;
                RestoreColor();
            }
        }

        public void Bind(IEventBus bus)
        {
            _bus = bus;
            Publish(0, 0, CollisionCategory.None);
        }

        public void SetAcceptingHits(bool accepting)
        {
            _accepting = accepting;
        }

        public void ResetRun()
        {
            _defeated = false;
            _immuneUntil = 0f;
            _flashUntil = 0f;
            _current = MaxHealth;
            RestoreColor();
            Publish(0, 0, CollisionCategory.None);
        }

        public void ApplyDamage(int amount)
        {
            if (amount <= 0)
                return;

            ApplyImpact(amount, 0, CollisionCategory.None);
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || _defeated)
                return;

            int before = _current;
            _current = Mathf.Min(MaxHealth, _current + amount);
            int gained = _current - before;
            if (gained <= 0)
                return;

            Publish(gained, 0, CollisionCategory.None);
        }

        public void ReceiveHit(CollisionCategory category)
        {
            if (_config == null)
            {
                ReportMissingConfig();
                return;
            }

            if (category == CollisionCategory.None || category == CollisionCategory.Collectable)
                return;

            _config.GetResponse(category, out int healthDamage, out int segments, out int healthWithoutSegments);
            int removed = segments > 0 && _body != null ? _body.RemoveSegments(segments) : 0;
            if (healthDamage <= 0 && removed == 0 && segments > 0)
                healthDamage = healthWithoutSegments;

            ApplyImpact(healthDamage, removed, category);
        }

        void ApplyImpact(int healthDamage, int segmentsLost, CollisionCategory category)
        {
            if (_defeated || !_accepting || IsImmune)
                return;

            if (healthDamage <= 0 && segmentsLost <= 0)
                return;

            if (healthDamage > 0)
                _current = Mathf.Max(0, _current - healthDamage);

            BeginImmunity();
            BeginFlash();
            Publish(-healthDamage, segmentsLost, category);

            if (_current > 0)
                return;

            _defeated = true;
            _bus?.Publish(new GameOverRequested("health"));
        }

        void BeginImmunity()
        {
            float seconds = _config != null ? Mathf.Max(0f, _config.ImmunitySeconds) : 0.75f;
            _immuneUntil = Time.time + seconds;
        }

        void BeginFlash()
        {
            float seconds = _config != null ? Mathf.Max(0f, _config.FlashSeconds) : 0.18f;
            _flashUntil = Time.unscaledTime + seconds;
            Color color = _config != null ? _config.DamageColor : new Color(1f, 0.28f, 0.22f, 1f);
            ApplyColor(color);
        }

        void Publish(int delta, int segmentsLost, CollisionCategory source)
        {
            var payload = new HealthChanged(_current, MaxHealth, delta, segmentsLost, source);
            Changed?.Invoke(payload);
            _bus?.Publish(payload);
        }

        void CacheBaseColor()
        {
            if (_renderer == null || _renderer.sharedMaterial == null)
                return;

            var material = _renderer.sharedMaterial;
            if (material.HasProperty(BaseColorId))
                _baseColor = material.GetColor(BaseColorId);
            else if (material.HasProperty(ColorId))
                _baseColor = material.GetColor(ColorId);

            _hasBaseColor = true;
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

        void RestoreColor()
        {
            if (_renderer == null)
                return;

            if (!_hasBaseColor)
            {
                _renderer.SetPropertyBlock(null);
                return;
            }

            ApplyColor(_baseColor);
        }

        void ReportMissingConfig()
        {
            if (_reportedMissingConfig)
                return;

            _reportedMissingConfig = true;
            Debug.LogError("[SerpentRush] SnakeHealth is missing a Health Config.", this);
        }
    }
}
