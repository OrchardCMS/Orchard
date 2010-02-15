using System;
using NHibernate;

namespace Orchard.Data {
    public interface ISessionLocator : IDependency {        
        ISession For(Type entityType);
    }
}