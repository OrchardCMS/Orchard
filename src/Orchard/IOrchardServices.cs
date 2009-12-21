using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.UI.Notify;

namespace Orchard {
    public interface IOrchardServices : IDependency {
        IContentManager ContentManager { get; }
        ITransactionManager TransactionManager { get; }
        IAuthorizer Authorizer { get; set; }
        INotifier Notifier { get; }
    }
}
