using NHibernate;

namespace Orchard.Data.Providers {
    public interface IDataServicesProviderFactory : IDependency {
        IDataServicesProvider CreateProvider(DataServiceParameters sessionFactoryParameters);
    }

    public static class IDataServicesProviderSelectorExtensions {
        public static ISessionFactory BuildSessionFactory(this IDataServicesProviderFactory factory, SessionFactoryParameters sessionFactoryParameters) {
            var provider = factory.CreateProvider(sessionFactoryParameters);
            return provider != null ? provider.BuildSessionFactory(sessionFactoryParameters) : null;
        }

    }
}
