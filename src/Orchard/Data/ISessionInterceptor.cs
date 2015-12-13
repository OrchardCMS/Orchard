using NHibernate;

namespace Orchard.Data {
    /// <summary>
    /// Describes an NHibernate session interceptor, instantiated per-session.
    /// </summary>
    public interface ISessionInterceptor : IInterceptor, IDependency {
    }
}