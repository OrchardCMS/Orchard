using System;
using FluentNHibernate.Cfg;
using NHibernate;
using NHibernate.Cfg;

namespace Orchard.Data.Providers {
    public interface IDataServicesProvider : ITransientDependency {
        ISessionFactory BuildSessionFactory(SessionFactoryParameters sessionFactoryParameters);
    }
}
