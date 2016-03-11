using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NHibernate;
using NHibernate.SqlCommand;
using NHibernate.Type;
using Orchard.ContentManagement;
using Orchard.Exceptions;
using Orchard.Logging;
using Orchard.Security;

namespace Orchard.Data {

    public class SessionLocator : ISessionLocator {
        private readonly ITransactionManager _transactionManager;

        public SessionLocator(ITransactionManager transactionManager) {
            _transactionManager = transactionManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public ISession For(Type entityType) {
            Logger.Debug("Acquiring session for {0}", entityType);
            return _transactionManager.GetSession();
        }
    }

    public class TransactionManager : ITransactionManager, IDisposable {
        private readonly ISessionFactoryHolder _sessionFactoryHolder;
        private readonly IEnumerable<ISessionInterceptor> _interceptors;
        private Func<IContentManagerSession> _contentManagerSessionFactory;

        private ISession _session;
        private IContentManagerSession _contentManagerSession;

        public TransactionManager(
            ISessionFactoryHolder sessionFactoryHolder,
            Func<IContentManagerSession> contentManagerSessionFactory,
            IEnumerable<ISessionInterceptor> interceptors) {
            _sessionFactoryHolder = sessionFactoryHolder;
            _interceptors = interceptors;
            _contentManagerSessionFactory = contentManagerSessionFactory;

            Logger = NullLogger.Instance;
            IsolationLevel = IsolationLevel.ReadCommitted;
        }

        public ILogger Logger { get; set; }
        public IsolationLevel IsolationLevel { get; set; }

        public ISession GetSession() {
            Demand();
            return _session;
        }
        public void Demand() {
            EnsureSession(IsolationLevel);
        }

        public void RequireNew() {
            RequireNew(IsolationLevel);
        }

        public void RequireNew(IsolationLevel level) {
            DisposeSession();
            EnsureSession(level);
        }

        public void Cancel() {
            // IsActive is true if the transaction hasn't been committed or rolled back
            if (_session != null && _session.Transaction.IsActive) {
                Logger.Debug("Rolling back transaction");
                _session.Transaction.Rollback();
                DisposeSession();
            }
        }

        public void Dispose() {
            DisposeSession();
        }

        private void DisposeSession() {
            if (_session != null) {

                try {
                    // IsActive is true if the transaction hasn't been committed or rolled back
                    if (_session.Transaction.IsActive) {
                        Logger.Debug("Committing transaction");
                        _session.Transaction.Commit();
                    }
                }
                finally {
                    _contentManagerSession.Clear();

                    Logger.Debug("Disposing session");
                    _session.Close();
                    _session.Dispose();
                    _session = null;
                }
            }
        }

        private void EnsureSession(IsolationLevel level) {
            if (_session != null) {
                return;
            }

            var sessionFactory = _sessionFactoryHolder.GetSessionFactory();
            Logger.Debug("Opening NHibernate session");
            _session = sessionFactory.OpenSession(new OrchardSessionInterceptor(_interceptors.ToArray(), Logger));
            _session.BeginTransaction(level);
            _contentManagerSession = _contentManagerSessionFactory();
        }

        class OrchardSessionInterceptor : IInterceptor {
            private readonly ISessionInterceptor[] _interceptors;
            private readonly ILogger _logger;

            public OrchardSessionInterceptor(ISessionInterceptor[] interceptors, ILogger logger) {
                _interceptors = interceptors;
                _logger = logger;
            }

            bool IInterceptor.OnLoad(object entity, object id, object[] state, string[] propertyNames, IType[] types) {
                if (_interceptors.Length == 0) return false;
                return _interceptors.Invoke(i => i.OnLoad(entity, id, state, propertyNames, types), _logger).ToList().Any(r => r);
            }

            bool IInterceptor.OnFlushDirty(object entity, object id, object[] currentState, object[] previousState, string[] propertyNames, IType[] types) {
                if (_interceptors.Length == 0) return false;
                return _interceptors.Invoke(i => i.OnFlushDirty(entity, id, currentState, previousState, propertyNames, types), _logger).ToList().Any(r => r);
            }

            bool IInterceptor.OnSave(object entity, object id, object[] state, string[] propertyNames, IType[] types) {
                if (_interceptors.Length == 0) return false;
                return _interceptors.Invoke(i => i.OnSave(entity, id, state, propertyNames, types), _logger).ToList().Any(r => r);
            }

            void IInterceptor.OnDelete(object entity, object id, object[] state, string[] propertyNames, IType[] types) {
                if (_interceptors.Length == 0) return;
                _interceptors.Invoke(i => i.OnDelete(entity, id, state, propertyNames, types), _logger);
            }

            void IInterceptor.OnCollectionRecreate(object collection, object key) {
                if (_interceptors.Length == 0) return;
                _interceptors.Invoke(i => i.OnCollectionRecreate(collection, key), _logger);
            }

            void IInterceptor.OnCollectionRemove(object collection, object key) {
                if (_interceptors.Length == 0) return;
                _interceptors.Invoke(i => i.OnCollectionRemove(collection, key), _logger);
            }

            void IInterceptor.OnCollectionUpdate(object collection, object key) {
                if (_interceptors.Length == 0) return;
                _interceptors.Invoke(i => i.OnCollectionUpdate(collection, key), _logger);
            }

            void IInterceptor.PreFlush(ICollection entities) {
                if (_interceptors.Length == 0) return;
                _interceptors.Invoke(i => i.PreFlush(entities), _logger);
            }

            void IInterceptor.PostFlush(ICollection entities) {
                if (_interceptors.Length == 0) return;
                _interceptors.Invoke(i => i.PostFlush(entities), _logger);
            }

            bool? IInterceptor.IsTransient(object entity) {
                if (_interceptors.Length == 0) return null;
                return _interceptors.Invoke(i => i.IsTransient(entity), _logger).ToList().FirstOrDefault(c => c.HasValue && c.Value);
            }

            int[] IInterceptor.FindDirty(object entity, object id, object[] currentState, object[] previousState, string[] propertyNames, IType[] types) {
                if (_interceptors.Length == 0) return null;
                var retVal = _interceptors.Invoke(i => i.FindDirty(entity, id, currentState, previousState, propertyNames, types), _logger)
                    .Where(r => r != null)
                    .SelectMany(r => r)
                    .ToArray();

                return retVal.Length == 0 ? null : retVal;
            }

            object IInterceptor.Instantiate(string entityName, EntityMode entityMode, object id) {
                if (_interceptors.Length == 0) return null;
                return _interceptors.Invoke(i => i.Instantiate(entityName, entityMode, id), _logger).FirstOrDefault(r => r != null);
            }

            string IInterceptor.GetEntityName(object entity) {
                if (_interceptors.Length == 0) return null;
                return _interceptors.Invoke(i => i.GetEntityName(entity), _logger).FirstOrDefault(r => r != null);
            }

            object IInterceptor.GetEntity(string entityName, object id) {
                if (_interceptors.Length == 0) return null;
                return _interceptors.Invoke(i => i.GetEntity(entityName, id), _logger).FirstOrDefault(r => r != null);
            }

            void IInterceptor.AfterTransactionBegin(ITransaction tx) {
                if (_interceptors.Length == 0) return;
                _interceptors.Invoke(i => i.AfterTransactionBegin(tx), _logger);
            }

            void IInterceptor.BeforeTransactionCompletion(ITransaction tx) {
                if (_interceptors.Length == 0) return;
                _interceptors.Invoke(i => i.BeforeTransactionCompletion(tx), _logger);
            }

            void IInterceptor.AfterTransactionCompletion(ITransaction tx) {
                if (_interceptors.Length == 0) return;
                _interceptors.Invoke(i => i.AfterTransactionCompletion(tx), _logger);
            }

            SqlString IInterceptor.OnPrepareStatement(SqlString sql) {
                if (_interceptors.Length == 0) return sql;

                // Cannot use Invoke, as we need to pass previous result to the next interceptor
                return _interceptors.Aggregate(sql, (current, i) => {
                    try {
                        return i.OnPrepareStatement(current);
                    }
                    catch (Exception ex) {
                        if (IsLogged(ex)) {
                            _logger.Error(ex, "{2} thrown from ISessionInterceptor by {0}",
                                i.GetType().FullName,
                                ex.GetType().Name);
                        }

                        if (ex.IsFatal()) {
                            throw;
                        }

                        return current;
                    }
                });
            }

            void IInterceptor.SetSession(ISession session) {
                if (_interceptors.Length == 0) return;
                _interceptors.Invoke(i => i.SetSession(session), _logger);
            }

            private static bool IsLogged(Exception ex) {
                return ex is OrchardSecurityException || !ex.IsFatal();
            }
        }
    }
}