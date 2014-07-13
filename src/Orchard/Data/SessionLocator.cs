using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NHibernate;
using NHibernate.SqlCommand;
using NHibernate.Type;
using Orchard.Exceptions;
using Orchard.Logging;
using Orchard.Security;

namespace Orchard.Data {
    public class SessionLocator : ISessionLocator, ITransactionManager, IDisposable {
        private readonly ISessionFactoryHolder _sessionFactoryHolder;
        private readonly IEnumerable<ISessionInterceptor> _interceptors;
        private ISession _session;
        private ITransaction _transaction;
        private bool _cancelled;

        public SessionLocator(
            ISessionFactoryHolder sessionFactoryHolder, 
            IEnumerable<ISessionInterceptor> interceptors) {
            _sessionFactoryHolder = sessionFactoryHolder;
            _interceptors = interceptors;
            Logger = NullLogger.Instance;
            IsolationLevel = IsolationLevel.ReadCommitted;
        }

        public ILogger Logger { get; set; }
        public IsolationLevel IsolationLevel { get; set; }

        public ISession For(Type entityType) {
            Logger.Debug("Acquiring session for {0}", entityType);

            ((ITransactionManager)this).Demand();

            return _session;
        }

        public void Demand() {
            EnsureSession();

            if (_transaction == null) {
                Logger.Debug("Creating transaction on Demand");
                _transaction = _session.BeginTransaction(IsolationLevel);
            }
        }

        public void RequireNew() {
            RequireNew(IsolationLevel);
        }

        public void RequireNew(IsolationLevel level) {
            EnsureSession();

            if (_cancelled) {
                if (_transaction != null) {
                    _transaction.Rollback();
                    _transaction.Dispose();
                    _transaction = null;
                }

                _cancelled = false;
            }
            else {
                if (_transaction != null) {
                    _transaction.Commit();
                }
            }

            Logger.Debug("Creating new transaction with isolation level {0}", level);
            _transaction = _session.BeginTransaction(level);
        }

        public void Cancel() {
            Logger.Debug("Transaction cancelled flag set");
            _cancelled = true;
        }

        public void Dispose() {
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
                }
                catch (Exception e) {
                    Logger.Error(e, "Error while disposing the transaction.");
                }
                finally {
                    _transaction.Dispose();
                    Logger.Debug("Transaction disposed");

                    _transaction = null;
                    _cancelled = false;
                }
            }

            if (_session != null) {
                _session.Dispose();
                _session = null;
            }

        }

        private void EnsureSession() {
            if (_session != null) {
                return;
            }

            var sessionFactory = _sessionFactoryHolder.GetSessionFactory();
            Logger.Information("Opening database session");
            _session = sessionFactory.OpenSession(new OrchardSessionInterceptor(_interceptors.ToArray(), Logger));
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