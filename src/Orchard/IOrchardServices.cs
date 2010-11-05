using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.UI.Notify;

namespace Orchard {
    public interface IOrchardServices : IDependency {
        IContentManager ContentManager { get; }
        ITransactionManager TransactionManager { get; }
        IAuthorizer Authorizer { get; }
        INotifier Notifier { get; }
        dynamic New { get; }
        WorkContext WorkContext { get; }
    }
}
