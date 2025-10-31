using System;
using Autofac;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Security;
using Orchard.UI.Notify;

namespace Orchard.Tokens.Tests
{
    public class StubOrchardServices : IOrchardServices
    {
        private readonly ILifetimeScope _lifetimeScope;

        public StubOrchardServices() { }

        public StubOrchardServices(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public IContentManager ContentManager => throw new NotImplementedException();

        public ITransactionManager TransactionManager => throw new NotImplementedException();

        public IAuthorizer Authorizer => throw new NotImplementedException();

        public INotifier Notifier => throw new NotImplementedException();

        public dynamic New => throw new NotImplementedException();

        private WorkContext _workContext;
        public WorkContext WorkContext
        {
            get
            {
                if (_workContext == null)
                {
                    _workContext = new StubWorkContextAccessor(_lifetimeScope).GetContext();
                }

                return _workContext;
            }
        }
    }
}
