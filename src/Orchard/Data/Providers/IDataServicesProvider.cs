using NHibernate;

namespace Orchard.Data.Providers {
    public interface IDataServicesProvider : ITransientDependency {
        ISessionFactory BuildSessionFactory(SessionFactoryParameters sessionFactoryParameters);
    }
}
