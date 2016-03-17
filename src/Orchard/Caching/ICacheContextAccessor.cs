namespace Orchard.Caching {
    public interface ICacheContextAccessor {
        IAcquireContext Current { get; set; }
    }
}