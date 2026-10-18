using UnityEngine;

namespace SerpentRush.Gameplay
{
    public interface ISteerInput
    {
        Vector2 Read();

        void Enable();

        void Disable();
    }
}
