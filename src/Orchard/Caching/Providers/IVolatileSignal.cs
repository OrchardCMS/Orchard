namespace Orchard.Caching.Providers {
    public interface IVolatileSignal {
        IVolatileProvider Provider { get; set; }
    }
}