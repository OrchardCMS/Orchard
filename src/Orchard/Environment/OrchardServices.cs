using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.UI.Notify;

namespace Orchard.Environment {
    [UsedImplicitly]
    public class OrchardServices : IOrchardServices {
        public OrchardServices(
            IContentManager contentManager,
            ITransactionManager transactionManager,
            IAuthorizer authorizer,
            INotifier notifier) {
            ContentManager = contentManager;
            TransactionManager = transactionManager;
            Authorizer = authorizer;
            Notifier = notifier;
        }
        public IContentManager ContentManager { get; set; }
        public ITransactionManager TransactionManager {get;set;}
        public IAuthorizer Authorizer { get; set; }
        public INotifier Notifier { get; set; }
    }
}
