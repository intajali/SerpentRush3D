using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Reads a Vector2 steer action. The action is owned by the caller.
    /// </summary>
    public sealed class DeviceSteerInput : ISteerInput
    {
        readonly InputAction _action;

        public DeviceSteerInput(InputAction action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public Vector2 Read()
        {
            return _action.ReadValue<Vector2>();
        }

        public void Enable()
        {
            _action.Enable();
        }

        public void Disable()
        {
            _action.Disable();
        }
    }
}
