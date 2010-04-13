using Orchard.ContentManagement;
using Orchard.Security.Permissions;

namespace Orchard.Security {
    public interface IAuthorizationServiceEvents : IEvents {
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

    public abstract class AuthorizationServiceEvents : IAuthorizationServiceEvents {
        public virtual void Checking(CheckAccessContext context) { }
        public virtual void Adjust(CheckAccessContext context) { }
        public virtual void Complete(CheckAccessContext context) { }
    }
}
