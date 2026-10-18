using SerpentRush.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace SerpentRush.UI
{
    /// <summary>
    /// Screen overlay for core health, energy, and body length. Graphics do not receive clicks.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Serpent Rush/Run HUD")]
    public sealed class RunHud : MonoBehaviour
    {
        [SerializeField] int _sortingOrder = 20;
        [SerializeField] float _margin = 28f;

        SnakeHealth _health;
        SnakeGrowthController _growth;
        SnakeBodyController _body;
        SnakeMovement _movement;
        GameObject _canvasObject;
        RectTransform _panel;
        Text _healthLabel;
        Text _energyLabel;
        Text _lengthLabel;
        RectTransform _healthFill;
        Image _healthFillImage;
        float _barWidth = 424f;
        int _shownHealth = -1;
        int _shownMax = -1;
        int _shownEnergy = -1;
        int _shownProgress = -1;
        int _shownThreshold = -1;
        int _shownLength = -1;
        int _shownSpeedTenths = -1;
        int _threshold = 5;
        bool _healthHooked;
        bool _growthHooked;

        void Awake()
        {
            Build();
        }

        void OnEnable()
        {
            Subscribe();
            Refresh();
        }

        void Start()
        {
            ApplySafeArea();
            Subscribe();
            Refresh();
        }

        void LateUpdate()
        {
            Subscribe();
            Refresh();
        }

        void OnDisable()
        {
            Unsubscribe();
        }

        void OnDestroy()
        {
            if (_canvasObject != null)
                Destroy(_canvasObject);
        }

        void Subscribe()
        {
            if (_health == null)
                _health = FindAnyObjectByType<SnakeHealth>();

            if (_growth == null)
                _growth = FindAnyObjectByType<SnakeGrowthController>();

            if (_body == null && _growth != null)
                _body = _growth.GetComponent<SnakeBodyController>();

            if (_body == null)
                _body = FindAnyObjectByType<SnakeBodyController>();

            if (_movement == null && _body != null)
                _movement = _body.GetComponent<SnakeMovement>();

            if (!_healthHooked && _health != null)
            {
                _health.Changed += OnHealthChanged;
                _healthHooked = true;
            }

            if (!_growthHooked && _growth != null)
            {
                _growth.Changed += OnEnergyChanged;
                _growthHooked = true;
            }
        }

        void Unsubscribe()
        {
            if (_healthHooked && _health != null)
                _health.Changed -= OnHealthChanged;

            if (_growthHooked && _growth != null)
                _growth.Changed -= OnEnergyChanged;

            _healthHooked = false;
            _growthHooked = false;
        }

        void OnHealthChanged(HealthChanged change)
        {
            ApplyHealth(change.Current, change.Max);
        }

        void OnEnergyChanged(EnergyChanged change)
        {
            _threshold = Mathf.Max(1, change.GrowthThreshold);
            ApplyEnergy(change.TotalEnergy, change.GrowthProgress, _threshold);
        }

        void Refresh()
        {
            if (_health != null)
                ApplyHealth(_health.CurrentHealth, _health.MaxHealth);

            if (_growth != null)
            {
                int progress = _growth.TotalEnergy % Mathf.Max(1, _threshold);
                ApplyEnergy(_growth.TotalEnergy, progress, _threshold);
            }

            if (_body != null)
                ApplyLength(_body.SegmentCount, _movement != null ? _movement.CurrentSpeed : 0f);
        }

        void ApplyHealth(int current, int max)
        {
            max = Mathf.Max(1, max);
            current = Mathf.Clamp(current, 0, max);
            if (current == _shownHealth && max == _shownMax)
                return;

            _shownHealth = current;
            _shownMax = max;
            if (_healthLabel != null)
                _healthLabel.text = "Health  " + current + " / " + max;

            float ratio = current / (float)max;
            if (_healthFill != null)
                _healthFill.sizeDelta = new Vector2(_barWidth * ratio, 0f);

            if (_healthFillImage != null)
            {
                _healthFillImage.color = Color.Lerp(
                    new Color(0.92f, 0.22f, 0.18f, 1f),
                    new Color(0.32f, 0.86f, 0.42f, 1f),
                    ratio);
            }
        }

        void ApplyEnergy(int total, int progress, int threshold)
        {
            if (total == _shownEnergy && progress == _shownProgress && threshold == _shownThreshold)
                return;

            _shownEnergy = total;
            _shownProgress = progress;
            _shownThreshold = threshold;
            if (_energyLabel != null)
                _energyLabel.text = "Energy  " + total + "    Next  " + progress + " / " + threshold;
        }

        void ApplyLength(int length, float speed)
        {
            int speedTenths = Mathf.RoundToInt(speed * 10f);
            if (length == _shownLength && speedTenths == _shownSpeedTenths)
                return;

            _shownLength = length;
            _shownSpeedTenths = speedTenths;
            if (_lengthLabel != null)
                _lengthLabel.text = "Length  " + length + "    Speed  " + speed.ToString("0.0");
        }

        void ApplySafeArea()
        {
            if (_panel == null)
                return;

            var canvas = _panel.GetComponentInParent<Canvas>();
            if (canvas == null)
                return;

            Canvas.ForceUpdateCanvases();
            float scale = canvas.scaleFactor;
            if (scale < 0.01f)
                scale = 1f;

            var safe = Screen.safeArea;
            float topInset = (Screen.height - safe.yMax) / scale;
            float leftInset = safe.xMin / scale;
            _panel.anchoredPosition = new Vector2(_margin + leftInset, -(_margin + topInset));
        }

        void Build()
        {
            _canvasObject = new GameObject("Run HUD Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = _canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = _sortingOrder;

            var scaler = _canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var root = (RectTransform)_canvasObject.transform;
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            var panelImage = CreateImage("Panel", root, new Color(0.04f, 0.07f, 0.1f, 0.78f));
            _panel = panelImage.rectTransform;
            _panel.anchorMin = new Vector2(0f, 1f);
            _panel.anchorMax = new Vector2(0f, 1f);
            _panel.pivot = new Vector2(0f, 1f);
            _panel.sizeDelta = new Vector2(460f, 150f);
            _panel.anchoredPosition = new Vector2(_margin, -_margin);

            _healthLabel = CreateLabel("Health Label", _panel, new Color(0.9f, 0.95f, 0.92f, 1f), 28);
            Place(_healthLabel.rectTransform, 18f, -14f, 424f, 34f);

            var bar = CreateImage("Health Back", _panel, new Color(1f, 1f, 1f, 0.16f));
            Place(bar.rectTransform, 18f, -54f, 424f, 18f);

            var fill = CreateImage("Health Fill", bar.rectTransform, new Color(0.32f, 0.86f, 0.42f, 1f));
            _healthFillImage = fill;
            _healthFill = fill.rectTransform;
            _barWidth = bar.rectTransform.sizeDelta.x;
            _healthFill.anchorMin = new Vector2(0f, 0f);
            _healthFill.anchorMax = new Vector2(0f, 1f);
            _healthFill.pivot = new Vector2(0f, 0.5f);
            _healthFill.anchoredPosition = Vector2.zero;
            _healthFill.sizeDelta = new Vector2(_barWidth, 0f);

            _energyLabel = CreateLabel("Energy Label", _panel, new Color(1f, 0.86f, 0.35f, 1f), 24);
            Place(_energyLabel.rectTransform, 18f, -80f, 424f, 30f);

            _lengthLabel = CreateLabel("Length Label", _panel, new Color(0.85f, 0.92f, 1f, 1f), 24);
            Place(_lengthLabel.rectTransform, 18f, -112f, 424f, 30f);

            ApplyHealth(0, 1);
            ApplyEnergy(0, 0, _threshold);
            ApplyLength(0, 0f);
        }

        static Image CreateImage(string name, Transform parent, Color color)
        {
            var host = new GameObject(name, typeof(RectTransform), typeof(Image));
            host.transform.SetParent(parent, false);
            var image = host.GetComponent<Image>();
            image.sprite = White();
            image.type = Image.Type.Simple;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        static Text CreateLabel(string name, Transform parent, Color color, int size)
        {
            var host = new GameObject(name, typeof(RectTransform), typeof(Text));
            host.transform.SetParent(parent, false);
            var text = host.GetComponent<Text>();
            text.font = BuiltinFont();
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleLeft;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        static void Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(x, y);
        }

        static Font BuiltinFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return font;
        }

        static Sprite _white;

        static Sprite White()
        {
            if (_white != null)
                return _white;

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply(false, false);
            _white = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            return _white;
        }
    }
}
