using System.Security.Principal;
using Orchard.Security;

namespace Orchard.ImportExport.Security {
    public class ApiIdentity : IIdentity {
        private readonly IUser _user;
        public ApiIdentity(IUser user) {
            _user = user;
        }

        public string Name { get { return _user.UserName; } }
        public string AuthenticationType { get { return "Api"; } }
        public bool IsAuthenticated { get { return true; } }
    }
}