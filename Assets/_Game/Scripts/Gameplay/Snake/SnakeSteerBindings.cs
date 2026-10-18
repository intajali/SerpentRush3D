using UnityEngine.InputSystem;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Default steer bindings for editor play. WASD, arrow keys, and the gamepad left stick.
    /// </summary>
    public static class SnakeSteerBindings
    {
        public static InputAction Create()
        {
            var steer = new InputAction("Steer", InputActionType.Value, expectedControlType: "Vector2");
            AddAxes(steer, "<Keyboard>/w", "<Keyboard>/s", "<Keyboard>/a", "<Keyboard>/d");
            AddAxes(steer, "<Keyboard>/upArrow", "<Keyboard>/downArrow", "<Keyboard>/leftArrow", "<Keyboard>/rightArrow");
            steer.AddBinding("<Gamepad>/leftStick");
            return steer;
        }

        static void AddAxes(InputAction steer, string up, string down, string left, string right)
        {
            steer.AddCompositeBinding("2DVector")
                .With("Up", up)
                .With("Down", down)
                .With("Left", left)
                .With("Right", right);
        }
    }
}
