namespace Orchard.Caching.Providers {
    public interface IVolatileProvider : IDependency {
        void Enlist(IVolatileSink sink);
    }
}
