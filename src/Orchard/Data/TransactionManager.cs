using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web.Mvc;
using Orchard.Mvc.Filters;

namespace Orchard.Data {
    public interface ITransactionManager : IDependency {
        void Cancel();
    }

    public class TransactionManager : ITransactionManager, IDisposable {
        private readonly TransactionScope _scope;
        private bool _cancelled;

        public TransactionManager() {
            _scope = new TransactionScope(TransactionScopeOption.Required);
        }

        void IDisposable.Dispose() {
            if (!_cancelled)
                _scope.Complete();
            _scope.Dispose();
        }

        void ITransactionManager.Cancel() {
            _cancelled = true;
        }
    }

    public class TransactionFilter : FilterProvider, IExceptionFilter{
        private readonly ITransactionManager _transactionManager;

        public TransactionFilter(ITransactionManager transactionManager) {
            _transactionManager = transactionManager;
        }

        public void OnException(ExceptionContext filterContext) {
            _transactionManager.Cancel();
        }
    }
}
