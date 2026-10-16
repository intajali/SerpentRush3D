namespace SerpentRush.Core.Pooling
{
    public interface IObjectPool<T> where T : class
    {
        int CountInactive { get; }

        int CountActive { get; }

        T Get();

        void Release(T item);

        void Clear();
    }
}
