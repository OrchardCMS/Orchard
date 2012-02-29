using System;
using System.Collections;
using NHibernate;
using NHibernate.SqlCommand;
using NHibernate.Type;
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
                _session = sessionFactory.OpenSession(new SessionInterceptor());
            }
            return _session;
        }

        class SessionInterceptor : IInterceptor {
            private ISession _session;

            bool IInterceptor.OnLoad(object entity, object id, object[] state, string[] propertyNames, IType[] types) {
                return false;
            }

            bool IInterceptor.OnFlushDirty(object entity, object id, object[] currentState, object[] previousState, string[] propertyNames, IType[] types) {
                return false;
            }

            bool IInterceptor.OnSave(object entity, object id, object[] state, string[] propertyNames, IType[] types) {
                return false;
            }

            void IInterceptor.OnDelete(object entity, object id, object[] state, string[] propertyNames, IType[] types) {
            }

            void IInterceptor.OnCollectionRecreate(object collection, object key) {
            }

            void IInterceptor.OnCollectionRemove(object collection, object key) {
            }

            void IInterceptor.OnCollectionUpdate(object collection, object key) {
            }

            void IInterceptor.PreFlush(ICollection entities) {
            }

            void IInterceptor.PostFlush(ICollection entities) {
            }

            bool? IInterceptor.IsTransient(object entity) {
                return null;
            }

            int[] IInterceptor.FindDirty(object entity, object id, object[] currentState, object[] previousState, string[] propertyNames, IType[] types) {
                return null;
            }

            object IInterceptor.Instantiate(string entityName, EntityMode entityMode, object id) {
                return null;
            }

            string IInterceptor.GetEntityName(object entity) {
                return null;
            }

            object IInterceptor.GetEntity(string entityName, object id) {
                return null;
            }

            void IInterceptor.AfterTransactionBegin(ITransaction tx) {
            }

            void IInterceptor.BeforeTransactionCompletion(ITransaction tx) {
            }

            void IInterceptor.AfterTransactionCompletion(ITransaction tx) {
            }

            SqlString IInterceptor.OnPrepareStatement(SqlString sql) {
                return sql;
            }

            void IInterceptor.SetSession(ISession session) {
                _session = session;
            }
        }
    }
}