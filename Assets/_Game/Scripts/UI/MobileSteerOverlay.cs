using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace SerpentRush.UI
{
    /// <summary>
    /// Builds a bottom-left on-screen stick for touch. Only that graphic raycasts, so buttons elsewhere still receive clicks.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Serpent Rush/Mobile Steer Overlay")]
    public sealed class MobileSteerOverlay : MonoBehaviour
    {
        [Tooltip("Must match the Steer binding. The stick writes a virtual gamepad, which Player/Steer already reads.")]
        [SerializeField] string _controlPath = "<Gamepad>/leftStick";

        [SerializeField, Min(10f)] float _movementRange = 80f;

        [Tooltip("Hit size of the stick, in reference pixels. Gameplay buttons must stay outside this corner.")]
        [SerializeField, Min(40f)] float _stickSize = 180f;

        [SerializeField, Min(0f)] float _screenMargin = 56f;

        [Tooltip("Keep this below gameplay button canvases so a button drawn on top of the stick wins the raycast.")]
        [SerializeField] int _canvasSortingOrder = 5;

        [SerializeField] bool _visible = true;

        RectTransform _well;
        GameObject _canvasObject;

        void Awake()
        {
            EnsureEventSystem();
            if (FindAnyObjectByType<OnScreenStick>() != null)
                return;

            Build();
        }

        void Start()
        {
            ApplySafeArea();
        }

        void OnDestroy()
        {
            if (_canvasObject != null)
                Destroy(_canvasObject);
        }

        void EnsureEventSystem()
        {
            var eventSystem = FindAnyObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                var host = new GameObject("EventSystem");
                host.AddComponent<EventSystem>();
                host.AddComponent<InputSystemUIInputModule>();
                return;
            }

            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        void Build()
        {
            _canvasObject = new GameObject("Mobile Controls Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

            var canvas = _canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = _canvasSortingOrder;
            canvas.pixelPerfect = false;

            var scaler = _canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var canvasRect = (RectTransform)_canvasObject.transform;
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;

            _well = CreateRect("Joystick", canvasRect);
            _well.anchorMin = Vector2.zero;
            _well.anchorMax = Vector2.zero;
            _well.pivot = new Vector2(0.5f, 0.5f);
            var wellSize = _stickSize + (_movementRange * 0.5f);
            _well.sizeDelta = new Vector2(wellSize, wellSize);
            _well.anchoredPosition = new Vector2(_screenMargin + (wellSize * 0.5f), _screenMargin + (wellSize * 0.5f));

            var baseDisc = CreateImage("Base", _well, Disc(), new Color(0.05f, 0.08f, 0.12f, 0.78f), false);
            PlaceCentered(baseDisc.rectTransform, _stickSize);
            var ring = CreateImage("Ring", _well, Ring(), new Color(1f, 1f, 1f, 0.95f), false);
            PlaceCentered(ring.rectTransform, _stickSize);

            var stickObject = new GameObject("Stick", typeof(RectTransform), typeof(Image));
            stickObject.transform.SetParent(_well, false);
            var stickRect = (RectTransform)stickObject.transform;
            PlaceCentered(stickRect, _stickSize);

            var stickImage = stickObject.GetComponent<Image>();
            stickImage.sprite = Disc();
            stickImage.color = new Color(1f, 1f, 1f, 0f);
            stickImage.raycastTarget = true;
            try
            {
                stickImage.alphaHitTestMinimumThreshold = 0.5f;
            }
            catch (System.Exception)
            {
                stickImage.alphaHitTestMinimumThreshold = 0f;
            }

            var knob = CreateImage("Handle", stickRect, Disc(), new Color(0.45f, 0.92f, 1f, 1f), false);
            PlaceCentered(knob.rectTransform, _stickSize * 0.42f);

            stickObject.SetActive(false);
            var stick = stickObject.AddComponent<OnScreenStick>();
            stick.controlPath = _controlPath;
            stick.behaviour = OnScreenStick.Behaviour.RelativePositionWithStaticOrigin;
            stick.movementRange = _movementRange;
            stick.useIsolatedInputActions = true;
            stickObject.SetActive(_visible);
            baseDisc.enabled = _visible;
            ring.enabled = _visible;
            knob.enabled = _visible;
        }

        void ApplySafeArea()
        {
            if (_well == null)
                return;

            var canvas = _well.GetComponentInParent<Canvas>();
            if (canvas == null)
                return;

            Canvas.ForceUpdateCanvases();
            var scale = canvas.scaleFactor;
            if (scale < 0.01f)
                return;

            var safe = Screen.safeArea;
            var position = _well.anchoredPosition;
            position.x += safe.xMin / scale;
            position.y += safe.yMin / scale;
            _well.anchoredPosition = position;
        }

        static RectTransform CreateRect(string name, Transform parent)
        {
            var host = new GameObject(name, typeof(RectTransform));
            host.transform.SetParent(parent, false);
            return (RectTransform)host.transform;
        }

        static Image CreateImage(string name, Transform parent, Sprite sprite, Color color, bool raycast)
        {
            var host = new GameObject(name, typeof(RectTransform), typeof(Image));
            host.transform.SetParent(parent, false);
            var image = host.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        static void PlaceCentered(RectTransform rect, float size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = Vector2.zero;
        }

        static Sprite Disc()
        {
            if (_disc == null)
                _disc = CreateCircle(false);
            return _disc;
        }

        static Sprite Ring()
        {
            if (_ring == null)
                _ring = CreateCircle(true);
            return _ring;
        }

        static Sprite CreateCircle(bool ring)
        {
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            var pixels = new Color32[size * size];
            var center = (size - 1) * 0.5f;
            var radius = center - 0.5f;
            var inner = radius * 0.86f;

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = x - center;
                    var dy = y - center;
                    var distance = Mathf.Sqrt((dx * dx) + (dy * dy));
                    var outside = Mathf.Clamp01(radius - distance + 0.5f);
                    var alpha = ring ? Mathf.Min(outside, Mathf.Clamp01(distance - inner + 0.5f)) : outside;
                    pixels[(y * size) + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(alpha) * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        static Sprite _disc;
        static Sprite _ring;
    }
}
