using UnityEngine;

namespace SerpentRush.Gameplay
{
    /// <summary>
    /// Converts a stick or key axis into a desired heading on the XZ plane.
    /// </summary>
    public static class SteerAim
    {
        public static Vector3 ToWorld(Vector2 steer, ISnakeMovementConfig config, Transform view)
        {
            if (config == null)
                return Vector3.zero;

            var deadzone = config.InputDeadzone;
            if (steer.sqrMagnitude < deadzone * deadzone)
                return Vector3.zero;

            var right = Vector3.right;
            var forward = Vector3.forward;
            if (config.CameraRelativeSteering && view != null)
                GetPlanarAxes(view, out right, out forward);

            var world = right * steer.x + forward * steer.y;
            world.y = 0f;
            if (world.sqrMagnitude < 0.0001f)
                return Vector3.zero;

            return world.normalized;
        }

        static void GetPlanarAxes(Transform view, out Vector3 right, out Vector3 forward)
        {
            var lookingDown = Mathf.Abs(Vector3.Dot(view.forward, Vector3.up)) > 0.85f;
            forward = Planar(lookingDown ? view.up : view.forward);
            right = Planar(view.right);

            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.forward;
            else
                forward.Normalize();

            if (right.sqrMagnitude < 0.0001f)
                right = Vector3.right;
            else
                right.Normalize();
        }

        public static Vector3 Planar(Vector3 value)
        {
            value.y = 0f;
            return value;
        }
    }
}
