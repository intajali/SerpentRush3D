namespace SerpentRush.Core.Factories
{
    /// <summary>
    /// Creates one product. Callers depend on this contract instead of a concrete constructor.
    /// </summary>
    public interface IFactory<out T>
    {
        T Create();
    }
}
