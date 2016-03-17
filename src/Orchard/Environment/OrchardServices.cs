using System;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Security;
using Orchard.UI.Notify;

namespace Orchard.Environment {
    public class OrchardServices : IOrchardServices {
        private readonly Lazy<IShapeFactory> _shapeFactory;
        private readonly IWorkContextAccessor _workContextAccessor;

        public OrchardServices(
            IContentManager contentManager,
            ITransactionManager transactionManager,
            IAuthorizer authorizer,
            INotifier notifier,
            Lazy<IShapeFactory> shapeFactory,
            IWorkContextAccessor workContextAccessor) {
            _shapeFactory = shapeFactory;
            _workContextAccessor = workContextAccessor;
            ContentManager = contentManager;
            TransactionManager = transactionManager;
            Authorizer = authorizer;
            Notifier = notifier;
        }

        public IContentManager ContentManager { get; private set; }
        public ITransactionManager TransactionManager { get; private set; }
        public IAuthorizer Authorizer { get; private set; }
        public INotifier Notifier { get; private set; }
        public dynamic New { get { return _shapeFactory.Value; } }
        public WorkContext WorkContext { get { return _workContextAccessor.GetContext(); } }
    }
}
