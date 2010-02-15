using System;
using NHibernate;
using Orchard.Logging;

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
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }


        public ISession For(Type entityType) {
            Logger.Debug("Acquiring session for {0}", entityType);

            if (_session == null) {

                var sessionFactory = _sessionFactoryHolder.GetSessionFactory();

                _transactionManager.Demand();

                Logger.Information("Openning database session");
                _session = sessionFactory.OpenSession();
            }
            return _session;
        }

    }
}