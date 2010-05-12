using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Security.Permissions;

namespace Orchard.Security {
    public interface IAuthorizationServiceEventHandler : IEventHandler {
        void Checking(CheckAccessContext context);
        void Adjust(CheckAccessContext context);
        void Complete(CheckAccessContext context);
    }

    public class CheckAccessContext {
        public Permission Permission { get; set; }
        public IUser User { get; set; }
        public IContent Content { get; set; }
        public bool Granted { get; set; }
        public bool Adjusted { get; set; }
    }
}
