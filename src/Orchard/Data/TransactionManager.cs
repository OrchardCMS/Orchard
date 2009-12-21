using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web.Mvc;
using Orchard.Mvc.Filters;

namespace Orchard.Data {
    public interface ITransactionManager : IDependency {
        void Demand();
        void Cancel();
    }

    public class TransactionManager : ITransactionManager, IDisposable {
        private TransactionScope _scope;
        private bool _cancelled;

        void ITransactionManager.Demand() {
            if (_scope == null) {
                _scope = new TransactionScope(TransactionScopeOption.Required);
            }
        }

        void ITransactionManager.Cancel() {
            _cancelled = true;
        }

        void IDisposable.Dispose() {
            if (_scope != null) {
                if (!_cancelled)
                    _scope.Complete();
                _scope.Dispose();
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
