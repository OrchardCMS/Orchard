
using System.Collections.Generic;

namespace Orchard.Security {
    public class IUserIdentityResult {

        public IUserIdentityResult(IUser user) {
            _user = user;
            _succeded = true;
        }

        public IUserIdentityResult(IUser user, List<string> errors) {
            _user = user;
            _errors = errors;
            _succeded = false;
        }

        private IUser _user;
        List<string> _errors;
        bool _succeded;

        public List<string> Errors { get { return _errors; } }
        public bool Succeeded { get { return _succeded; } }
        public IUser User { get { return _user; } }
    }
}