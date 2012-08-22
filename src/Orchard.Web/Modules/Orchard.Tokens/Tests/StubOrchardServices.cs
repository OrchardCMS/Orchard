using System;
using Autofac;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Security;
using Orchard.UI.Notify;

namespace Orchard.Tokens.Tests {
    public class StubOrchardServices : IOrchardServices {
        private readonly ILifetimeScope _lifetimeScope;

        public StubOrchardServices() { }

        public StubOrchardServices(ILifetimeScope lifetimeScope) {
            _lifetimeScope = lifetimeScope;
        }

        public IContentManager ContentManager {
            get { throw new NotImplementedException(); }
        }

        public ITransactionManager TransactionManager {
            get { throw new NotImplementedException(); }
        }

        public IAuthorizer Authorizer {
            get { throw new NotImplementedException(); }
        }

        public INotifier Notifier {
            get { throw new NotImplementedException(); }
        }

        public dynamic New {
            get { throw new NotImplementedException(); }
        }

        private WorkContext _workContext;
        public WorkContext WorkContext {
            get {
                if (_workContext == null) {
                    _workContext = new StubWorkContextAccessor(_lifetimeScope).GetContext();
                }

                return _workContext;
            }
        }
    }
}
