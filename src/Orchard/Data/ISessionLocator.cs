using System;
using NHibernate;

namespace Orchard.Data {
    public interface ISessionLocator {        
        ISession For(Type entityType);
    }
}