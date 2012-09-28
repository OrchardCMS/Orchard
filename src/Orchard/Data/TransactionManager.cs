using System;
using System.Transactions;
using System.Web.Mvc;
using Orchard.Logging;
using Orchard.Mvc.Filters;

namespace Orchard.Data {
    public interface ITransactionManager : IDependency {
        void Demand();
        void Cancel();
    }

    public class TransactionManager : ITransactionManager, IDisposable {
        private TransactionScope _scope;
        private bool _cancelled;

        public TransactionManager() {
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        void ITransactionManager.Demand() {
            if(_cancelled) {
                try {
                    _scope.Dispose();
                }
                catch {
                    // swallowing the exception
                }

                _scope = null;
            }

            if (_scope == null) {
                Logger.Debug("Creating transaction on Demand");
                _scope = new TransactionScope(
                    TransactionScopeOption.Required, 
                    new TransactionOptions { 
                        IsolationLevel = IsolationLevel.ReadCommitted 
                    });
            }
        }

        void ITransactionManager.Cancel() {
            Logger.Debug("Transaction cancelled flag set");
            _cancelled = true;
        }

        void IDisposable.Dispose() {
            if (_scope != null) {
                if (!_cancelled) {
                    Logger.Debug("Marking transaction as complete");
                    _scope.Complete();
                }

                Logger.Debug("Final work for transaction being performed");
                try {
                    _scope.Dispose();
                }
                catch {
                    // swallowing the exception
                }
                Logger.Debug("Transaction disposed");
            }
        }

    }

    public class TransactionFilter : FilterProvider, IExceptionFilter {
        private readonly ITransactionManager _transactionManager;

        public TransactionFilter(ITransactionManager transactionManager) {
            _transactionManager = transactionManager;
        }

        public void OnException(ExceptionContext filterContext) {
            _transactionManager.Cancel();
        }
    }
}
