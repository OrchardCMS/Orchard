namespace Orchard.Caching {
    public interface IVolatileToken {
        bool IsCurrent { get; }
    }
}