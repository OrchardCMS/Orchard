using System;
using System.Data;
using NHibernate;
using Orchard.Data;

namespace Orchard.Tests.ContentManagement {
    public class TestTransactionManager : ITransactionManager, IDisposable {
        private ISession _session;
        private ITransaction _transaction;
        private bool _cancelled;

        public TestTransactionManager(ISession session) {
            _session = session;
            RequireNew();
        }

        public void Demand() {
            EnsureSession();
        }

        public void RequireNew() {
            RequireNew(IsolationLevel.ReadCommitted);
        }

        public void RequireNew(IsolationLevel level) {
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

            _transaction = _session.BeginTransaction(level);
        }

        public void Cancel() {
            _cancelled = true;
        }

        public void Dispose() {
            if (_transaction != null) {
                try {
                    if (!_cancelled) {
                        _transaction.Commit();
                    }
                    else {
                        _transaction.Rollback();
                    }

                    _transaction.Dispose();
                }
                catch {
                }
                finally {
                    _transaction = null;
                    _cancelled = false;
                }
            }

            _session.Close();
            _session.Dispose();
            _session = null;
        }

        private void EnsureSession() {
            if (_session == null) {
                throw new ArgumentNullException("Session can't be null, ever");
            }
        }

        public ISession GetSession() {
            EnsureSession();

            return _session;
        }
    }
}
