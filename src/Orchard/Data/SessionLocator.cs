using System;
using NHibernate;

namespace Orchard.Data {
    public class SessionLocator : ISessionLocator {
        private readonly ISessionFactoryHolder _sessionFactoryHolder;
        private readonly ITransactionManager _transactionManager;
        private ISession _session;

        public SessionLocator(
            ISessionFactoryHolder sessionFactoryHolder,
            ITransactionManager transactionManager) {
            _sessionFactoryHolder = sessionFactoryHolder;
            _transactionManager = transactionManager;
        }


        public ISession For(Type entityType) {
            if (_session==null) {
                _transactionManager.Demand();
                var sessionFactory = _sessionFactoryHolder.GetSessionFactory();
                _session = sessionFactory.OpenSession();
            }
            return _session;
        }

    }
}