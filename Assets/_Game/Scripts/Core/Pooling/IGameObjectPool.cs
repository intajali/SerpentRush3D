using System;
using UnityEngine;

namespace SerpentRush.Core.Pooling
{
    public interface IGameObjectPool : IDisposable
    {
        int CountInactive { get; }

        int CountActive { get; }

        GameObject Get();

        GameObject Get(Vector3 position, Quaternion rotation);

        void Release(GameObject instance);

        void Clear();
    }
}
