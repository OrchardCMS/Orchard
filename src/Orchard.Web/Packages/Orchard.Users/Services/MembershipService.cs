using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Models;
using Orchard.Security;
using Orchard.Users.Models;

namespace Orchard.Users.Services {
    public class MembershipService : IMembershipService {
        private readonly IModelManager _modelManager;

        public MembershipService(IModelManager modelManager) {
            _modelManager = modelManager;
        }

        public void ReadSettings(MembershipSettings settings) {
            // accepting defaults
        }

        public IUser CreateUser(CreateUserParams createUserParams) {
            var user = _modelManager.New("user").As<UserModel>();
            user.UserName = createUserParams.Username;
            user.Email = createUserParams.Email;
            _modelManager.Create(user);
            return user;
        }

        public IUser GetUser(string username) {
            throw new NotImplementedException();
        }
    }
}
