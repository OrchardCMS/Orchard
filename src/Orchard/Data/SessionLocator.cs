using System;
using System.Collections;
using System.Data;
using NHibernate;
using NHibernate.SqlCommand;
using NHibernate.Type;
using Orchard.Logging;

namespace Orchard.Data {
    public class SessionLocator : ISessionLocator, ITransactionManager, IDisposable {
        private readonly ISessionFactoryHolder _sessionFactoryHolder;
        private ISession _session;
        private ITransaction _transaction;
        private bool _cancelled;

        public SessionLocator(ISessionFactoryHolder sessionFactoryHolder) {
            _sessionFactoryHolder = sessionFactoryHolder;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public ISession For(Type entityType) {
            Logger.Debug("Acquiring session for {0}", entityType);

            ((ITransactionManager)this).Demand();

            return _session;
        }

        void ITransactionManager.Demand() {
            EnsureSession();

            if (_transaction == null) {
                Logger.Debug("Creating transaction on Demand");
                _transaction = _session.BeginTransaction(IsolationLevel.ReadCommitted);
            }
        }

        void ITransactionManager.RequireNew() {
            ((ITransactionManager)this).RequireNew(IsolationLevel.ReadCommitted);
        }

        void ITransactionManager.RequireNew(IsolationLevel level) {
            EnsureSession();

            if (_cancelled) {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
            }
            else {
                if (_transaction != null) {
                    _transaction.Commit();
                }
            }

            Logger.Debug("Creating new transaction with isolation level {0}", level);
            _transaction = _session.BeginTransaction(level);
        }

        void ITransactionManager.Cancel() {
            Logger.Debug("Transaction cancelled flag set");
            _cancelled = true;
        }

        void IDisposable.Dispose() {
            if (_transaction != null) {
                try {
                    if (!_cancelled) {
                        Logger.Debug("Marking transaction as complete");
                        _transaction.Commit();
                    }
                    else {
                        Logger.Debug("Reverting operations from transaction");
                        _transaction.Rollback();
                    }

                    _transaction.Dispose();
                    Logger.Debug("Transaction disposed");
                }
                catch (Exception e) {
                    Logger.Error(e, "Error while disposing the transaction.");
                }
                finally {
                    _transaction = null;
                    _cancelled = false;
                }
            }
        }

        private void EnsureSession() {
            if (_session != null) {
                return;
            }

            var sessionFactory = _sessionFactoryHolder.GetSessionFactory();
            Logger.Information("Opening database session");
            _session = sessionFactory.OpenSession(new SessionInterceptor());
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