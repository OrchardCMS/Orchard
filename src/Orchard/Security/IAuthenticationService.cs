using System.Web;

namespace Orchard.Security {
    public interface IAuthenticationService : IDependency {
        void SignIn(IUser user, bool createPersistentCookie);
        void SignOut();
        IUser Authenticated();
    }
}
