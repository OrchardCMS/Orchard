using System;
using NHibernate;

namespace Orchard.Data {
    public interface ISessionLocator : IDependency {

        [Obsolete("Use ITransactionManager.GetSession() instead.")]
        ISession For(Type entityType);
    }
}